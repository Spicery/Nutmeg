"""
parser -- parser module for the Nutmeg compiler
"""

import codetree
from tokenizer import tokenizer, IdToken, BasicToken, CharToken, IntToken, StringToken, BoolToken, Token
from peekablepushable import PeekablePushable
import math
from mishap import Mishap

class TableDrivenParser:
    """
    Table-driven recursive descent parser. We use the category of the token
    to look up in tables what we should do next.
    """

    def __init__( self, prefix_table, postfix_table, unit=None ):
        self._prefix_table = prefix_table
        self._postfix_table = postfix_table
        self._non_breakable = True
        self._non_breakable_dump = []
        self._unit = unit

    def unit( self ):
        return self._unit

    def isNonBreakable( self ):
        return self._non_breakable

    def isBreakable( self ):
        return not self._non_breakable

    def pushNonBreakable( self, flag ):
        self._non_breakable_dump.append( self._non_breakable )
        self._non_breakable = flag

    def pushNonBreakableOr( self, flag ):
        self.pushNonBreakable( flag or self._non_breakable )

    def popNonBreakable( self ):
        self._non_breakable = self._non_breakable_dump.pop()

    def tryRunPrefixMiniParser( self, token, source ):
        # print( "PREFIX", token, token.category() )
        try:
            self.pushNonBreakableOr( token.isOutfixer() )
            minip = self._prefix_table[ token.category() ]
            return minip( self, token, source )
        except KeyError:
            return None
        finally:
            self.popNonBreakable()

    def runPostfixMiniParser( self, prec, lhs, token, source ):
        try:
            self.pushNonBreakableOr( token.isOutfixer() )
            try:
                # print( 'TOKEN', token, token.category(), token.isOutfixer(), self.isBreakable() )
                minip = self._postfix_table[ token.category() ]
            except KeyError:
                raise Mishap( f'Unexpected token in infix/postfix position', token=token.value(), position=token.span() )
            return minip( self, prec, lhs, token, source )
        finally:
            self.popNonBreakable()

    def readExpr( self, prec, source ):
        e = self.tryReadExpr( prec, source, checkNewlines=False )
        if e:
            return e
        elif source.isEmpty():
            raise Mishap( 'Unexpected end of input' )
        else:
            t = source.pop()
            raise Mishap( f'No continuation because of unexpected token', token=t.value(), position=t.span() )

    def tryReadExpr( self, prec, source, checkNewlines=True ):
        token = source.popOrElse()
        # print( 'TRYREADEXPR', token.value(), checkNewlines,token.followsNewLine() and self.isBreakable(), prec )
        if not token:
            return None
        elif checkNewlines:
            if token.followsNewLine() and self.isBreakable():
                return None
        sofar = self.tryRunPrefixMiniParser( token, source )
        if not sofar:
            source.push( token )
            return None
        while True:
            token = source.peekOrElse()
            # print( 'PEEK', token, token and token.isPostfixer(), token.followsNewLine() and self.isBreakable() )
            if not token or not token.isPostfixer(): break
            if token.followsNewLine() and self.isBreakable(): break
            p = token.precedence()
            # print( 'PREC', p, prec )
            if not p or not p <= prec: break
            source.pop()
            # print( 'Run postfix mini parser' )
            sofar = self.runPostfixMiniParser( p, sofar, token, source )
        return sofar

    @staticmethod
    def isntArgs( expr ):
        if isinstance( expr, codetree.IdCodelet ):
            return None
        elif isinstance( expr, codetree.SeqCodelet ):
            for i in expr.members():
                issue = TableDrivenParser.isntArgs( i )
                if issue:
                    return issue
            return None
        else:
            return f'Non-simple argument ({type(expr)})'

    @staticmethod
    def isntFuncArgs( expr ):
        if not isinstance( expr, codetree.CallCodelet ):
            return "Missing function call"
        func = expr.function()
        if not isinstance( func, codetree.IdCodelet ):
            return "Not a call of a simple variable"
        return TableDrivenParser.isntArgs( expr.arguments() )

    def readFuncArgs( self, source ):
        funcargs = self.readExpr( math.inf, source )
        issue = TableDrivenParser.isntFuncArgs( funcargs )
        if issue:
            raise Exception( f'Invalid expression for function definition ({issue})' )
        else:
            return funcargs

    def isVirtualSemi( self, nonsemi ):
        if not nonsemi.followsNewLine():
            return False
        if nonsemi.isPrefixerOnly():
            return True
        if nonsemi.isPostfixerOnly():
            return False
        # It depends on whether we're inside an outfixer.
        return self.isBreakable()

    def readStatements( self, source ):
        body = list( self.readStatementsGenerator( source ) )
        if len( body ) == 1:
            return body[0]
        else:
            return codetree.SeqCodelet( body=body )

    def readStatementsGenerator( self, source ):
        try:
            self.pushNonBreakable( False )
            while not source.isEmpty():
                e = self.tryReadExpr( math.inf, source, checkNewlines=False )
                if e:
                    yield e
                else:
                    break
                # Continue if there's a semi-colon o.n.o. - which means breaking if there isn't.
                if not tryRead( source, 'TERMINATE_STATEMENT' ):
                    # Break if we don't find a semicolon or a newline that counts as a semi-colon.
                    nonsemi = source.peekOrElse()
                    if nonsemi is None or not self.isVirtualSemi( nonsemi ):
                        break
        finally:
            self.popNonBreakable()

    def parseFromFileObject(self, file_object):
        return self.parseFromString( file_object.read(), origin=str(file_object) )

    def parseFromString( self, text, origin=None ):
        source = PeekablePushable( tokenizer( text ) )
        yield from self.readStatementsGenerator( source )
        if not source.isEmpty():
            t : Token = source.peekOrElse()
            raise Mishap( 'Unexpected token after end of statements', token=t.value(), position=t.positionInText() )

def mustRead( source, *categories, **kwargs ):
    token = source.popOrElse()
    if token:
        if token.category() not in categories:
            m = Mishap( f'Required keyword not found', found=token.value(), position=token.positionInText() )
        else:
            # Only clean exit
            return
    else:
        m = Mishap( 'Unexpected end of file' )
    if kwargs:
        m.addDetails( **kwargs )
    raise m

def tryRead( source, *categories ):
    token = source.peekOrElse()
    ok = token and token.category() in categories
    if ok:
        source.pop()
    return ok

################################################################################
### Set up the tables
################################################################################

def lparenPrefixMiniParser( parser, token, source ):
    if tryRead( source, 'RPAREN' ):
        return codetree.SeqCodelet()
    else:
        e = parser.readExpr( math.inf, source )
        mustRead( source, "RPAREN", expected=')', hint='Missing comma before this token?' )
        return e

def lbracketPrefixMiniParser( parser, token, source ):
    if tryRead( source, 'RBRACKET' ):
        kernel = codetree.SeqCodelet()
    else:
        kernel = parser.readExpr( math.inf, source )
        mustRead( source, "RBRACKET", expected=')', hint='Missing comma?' )
    return codetree.SyscallCodelet( name="newImmutableList", arguments=kernel )

def defPrefixMiniParser( parser, token, source ):
    funcArgs = parser.readFuncArgs( source )
    funcArgs.declarationMode()
    mustRead( source, 'END_PARAMETERS', 'END_PHRASE', expected=': or =>>' )
    b = parser.readStatements( source )
    mustRead( source, 'END_DEC_FUNCTION_1', 'END', expected='end or enddef' )
    func = funcArgs.function()
    args = funcArgs.arguments()
    id = codetree.IdCodelet( name=func.name(), reftype="val" )
    func = codetree.LambdaCodelet( parameters=args, body=b )
    return codetree.BindingCodelet( lhs=id, rhs=func )

def forPrefixMiniParser( parser, token, source ):
    # for ^ QUERY do STMNTS endfor
    query = parser.readExpr( math.inf, source )
    # for QUERY ^ do STMNTS endfor
    mustRead( source, 'DO', 'END_PHRASE', expected=': or do' )
    # for QUERY do ^ STMNTS endfor
    body = parser.readStatements( source )
    mustRead( source, "ENDFOR", "END", expected='end or endfor' )
    return codetree.ForCodelet( query=query, body=body )

def ifPrefixMiniParser( parser, token, source ):
    return ifXXXPrefixMiniParser( parser, token, source, negatedForm=False, closingKeyword="END_IF" )

def ifnotPrefixMiniParser( parser, token, source ):
    return ifXXXPrefixMiniParser( parser, token, source, negatedForm=True, closingKeyword="END_IFNOT" )

def ifXXXPrefixMiniParser( parser, token, source, *, negatedForm, closingKeyword ):
    # ifXXX ^ EXPR then STMNTS ... endifXXX
    testPart = parser.readExpr( math.inf, source )
    if negatedForm:
        testPart = codetree.SyscallCodelet(name="not",arguments=testPart)
    # ifXXX EXPR ^ then STMNTS ... endifXXX
    mustRead( source, "THEN", 'END_PHRASE', expected=': or then' )
    # ifXXX EXPR then ^ STMNTS ... endifXXX
    thenPart = parser.readStatements( source )
    # ifXXX EXPR then STMNTS ^ (elseif EXPR then STATEMENTS ... | else STMNTS | )  endifXXX
    if tryRead( source, "ELSE_IF" ):
        # ifXXX EXPR then STMNTS elseif ^ EXPR then STATEMENTS ... endifXXX
        elsePart = ifXXXPrefixMiniParser( parser, None, source, negatedForm=False, closingKeyword=closingKeyword )
        # ifXXX EXPR then STMNTS elseif EXPR then STATEMENTS .... endifXXX ^
        return codetree.IfCodelet( testPart=testPart, thenPart=thenPart, elsePart=elsePart )
    elif tryRead( source, "ELSE_IFNOT" ):
        # ifXXX EXPR then STMNTS elseif ^ EXPR then STATEMENTS ... endifXXX
        elsePart = ifXXXPrefixMiniParser( parser, None, source, negatedForm=True, closingKeyword=closingKeyword )
        # ifXXX EXPR then STMNTS elseif EXPR then STATEMENTS .... endifXXX ^
        return codetree.IfCodelet( testPart=testPart, thenPart=thenPart, elsePart=elsePart )
    elif tryRead( source, "ELSE" ):
        tryRead( source, 'END_PHRASE' )  ### Discard optional colon
        # ifXXX EXPR then STMNTS else ^ STMNTS endifXXX
        elsePart = parser.readStatements( source )
        # ifXXX EXPR then STMNTS else STMNTS ^ endifXXX
        mustRead( source, closingKeyword, "END" )
        # ifXXX EXPR then STMNTS else STMNTS endifXXX ^
        return codetree.IfCodelet( testPart=testPart, thenPart=thenPart, elsePart=elsePart )
    else:
        # ifXXX EXPR then STMNTS ^ endifXXX
        mustRead( source, closingKeyword, "END", expected=("end or endif" if closingKeyword == "END_IF" else 'end or endifnot') )
        # ifXXX EXPR then STMNTS endifXXX ^
        return codetree.IfCodelet( testPart=testPart, thenPart=thenPart, elsePart=codetree.SeqCodelet() )

def varPrefixMiniParser( parser, token, source ):
    t = source.pop()
    if t.category() == IdToken:
        return codetree.IdCodelet( name=t.value(), reftype=token.value() )
    else:
        raise Exception( f"Unexpected token: {t}")

def annotationPrefixMiniParser( parser, token, source ):
    t = source.pop()
    if t.category() == IdToken:
        e = parser.tryReadExpr( math.inf, source, checkNewlines=False )
        if not isinstance( e, codetree.BindingCodelet ):
            raise Mishap( 'Invalid expression following annotation, needed binding' )
        if t.value() == "unittest":
            e.setAnnotation( unittest=True )
        elif t.value() == "command":
            e.setAnnotation( command=True )
        else:
            raise Mishap( 'Unknown annotation', name=t.value() )
        return e
    else:
        raise Mishap( "Invalid name for annotation", name=t )

def tryMatchSyscall( name, expr ):
    while isinstance( expr, codetree.SeqCodelet ) and len( expr.body() ) == 1:
        expr = expr.body()[0]
    if isinstance( expr, codetree.SyscallCodelet ) and expr.name() == name:
        return expr
    else:
        return None

def assertPrefixMiniParser( parser, token, source ):
    expr = parser.tryReadExpr( math.inf, source, checkNewlines=False )
    position = codetree.IntCodelet( token.span()[0] )
    unit = codetree.StringCodelet( parser.unit() )
    e = tryMatchSyscall( "==", expr )
    if e:
        args = codetree.SeqCodelet( *e.arguments(), unit, position )
        return codetree.SyscallCodelet( name="assertEquals", arguments=args )
    e = tryMatchSyscall( "!=", expr )
    if e:
        args = codetree.SeqCodelet( *e.arguments(), unit, position )
        return codetree.SyscallCodelet( name="assertNotEquals", arguments=args )
    args = codetree.SeqCodelet( expr, unit, position )
    return codetree.SyscallCodelet( name="assertTrue", arguments=args )


PREFIX_TABLE = {
    "ANNOTATION": annotationPrefixMiniParser,
    "ASSERT": assertPrefixMiniParser,
    "LPAREN": lparenPrefixMiniParser,
    "LBRACKET": lbracketPrefixMiniParser,
    "DEC_FUNCTION_1": defPrefixMiniParser,
    "IF": ifPrefixMiniParser,
    "IFNOT": ifnotPrefixMiniParser,
    "FOR": forPrefixMiniParser,
    BasicToken: lambda parser, token, source: codetree.StringCodelet( value=token.value() ),
    IdToken: lambda parser, token, source: codetree.IdCodelet( name=token.value(), reftype="get" ),
    CharToken: lambda parser, token, source: codetree.CharCodelet( value=token.literalValue() ),
    IntToken: lambda parser, token, source: codetree.IntCodelet( value=str( token.valueAsInt() ) ),
    BoolToken: lambda parser, token, source: codetree.BoolCodelet( value=token.value() ),
    StringToken: lambda parser, token, source: codetree.StringCodelet( value=token.literalValue() ),
    "DEC_VARIABLE": varPrefixMiniParser,
    "DEC_NONASSIGNABLE": varPrefixMiniParser,
    "DEC_IMMUTABLE": varPrefixMiniParser,
}

def idPostfixMiniParser( parser, p, lhs, token, source ):
    rhs = parser.readExpr( p, source )
    return codetree.SyscallCodelet(
        name=token.value(),
        arguments=codetree.SeqCodelet( lhs, rhs )
    )

def commaPostfixMiniParser( parser, p, lhs, token, source ):
    sofar = [ lhs ]
    while True:
        rhs = parser.readExpr( p, source )
        sofar.append( rhs )
        if tryRead( source, token.category() ):
            next( source )
        else:
            break
    return codetree.SeqCodelet( *sofar )

def lparenPostfixMiniParser( parser, p, lhs, token, source ):
    if tryRead(source, 'RPAREN'):
        return codetree.CallCodelet( function=lhs, arguments=codetree.SeqCodelet() )
    else:
        rhs = parser.readExpr( math.inf, source )
        mustRead( source, "RPAREN", expected=')', hint='Missing comma before this token?' )
        return codetree.CallCodelet( function=lhs, arguments=rhs )

def bindPostfixMiniParser( parser, p, lhs, token, source ):
    lhs.declarationMode()
    rhs = parser.readExpr( p, source )
    return codetree.BindingCodelet( lhs=lhs, rhs=rhs )

def assignPostfixMiniParser( parser, p, lhs, token, source ):
    lhs.assignMode()
    rhs = parser.readExpr( p, source )
    return codetree.AssignCodelet( lhs=lhs, rhs=rhs )

def inPostfixMiniParser( parser, p, lhs, token, source ):
    lhs.declarationMode()
    rhs = parser.readExpr( math.inf, source )
    return codetree.InCodelet( pattern = lhs, streamable = rhs )

def dotPostfixMiniParser( parser : TableDrivenParser, p, lhs, token, source : PeekablePushable ):
    rhs = parser.readExpr( p-1, source )
    if isinstance( rhs, codetree.IdCodelet ):
        return codetree.CallCodelet(function=rhs, arguments=lhs)
    elif isinstance( rhs, codetree.CallCodelet ):
        args = codetree.SeqCodelet(lhs, rhs.arguments())
        return codetree.CallCodelet(function=rhs.function(),arguments=args)
    elif isinstance( rhs, codetree.SyscallCodelet ):
        args = codetree.SeqCodelet( lhs, rhs.arguments() )
        return codetree.SyscallCodelet(name=rhs.name(), arguments=args)
    else:
        raise Exception( "Unexpected expression after '.'" )

def andOrPostfixMiniParser( parser : TableDrivenParser, p, lhs, token, source : PeekablePushable ):
    rhs = parser.readExpr( p, source )
    make = codetree.AndCodelet if token.category() == "AND" else codetree.OrCodelet
    return make( lhs=lhs, rhs=rhs )

def discardPostfixMiniParser( parser : TableDrivenParser, p, lhs, token, source : PeekablePushable ):
    return codetree.SyscallCodelet( name="eraseAll", arguments=lhs )

POSTFIX_TABLE = {
    "SEQ": commaPostfixMiniParser,
    "DOT": dotPostfixMiniParser,
    "LPAREN": lparenPostfixMiniParser,
    IdToken: idPostfixMiniParser,
    "BIND": bindPostfixMiniParser,
    "ASSIGN": assignPostfixMiniParser,
    "IN": inPostfixMiniParser,
    "AND": andOrPostfixMiniParser,
    "OR": andOrPostfixMiniParser,
    "DISCARD": discardPostfixMiniParser,
}

def standardParser( unit=None ):
    return TableDrivenParser( PREFIX_TABLE, POSTFIX_TABLE, unit=unit )

def parseFromString( text ):
    return standardParser().parseFromString( text )

def parseFromFileObject( file_object, unit=None ):
    return standardParser( unit=unit ).parseFromFileObject( file_object )

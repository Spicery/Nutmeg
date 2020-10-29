"""
parser -- parser module for the Nutmeg compiler
"""

import codetree
from tokenizer import tokenizer, IdToken, BasicToken, IntToken, StringToken, BoolToken
from peekablepushable import PeekablePushable
import math
from mishap import Mishap

class TableDrivenParser:
    """
    Table-driven recursive descent parser. We use the category of the token
    to look up in tables what we should do next.
    """

    def __init__( self, prefix_table, postfix_table ):
        self._prefix_table = prefix_table
        self._postfix_table = postfix_table
        self._non_breakable = True
        self._non_breakable_dump = []

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
        try:
            self.pushNonBreakableOr( token.isOutfixer() )
            minip = self._prefix_table[ token.category() ]
            # print( "PREFIX", token )
            return minip( self, token, source )
        except KeyError:
            return None
        finally:
            self.popNonBreakable()

    def runPostfixMiniParser( self, prec, lhs, token, source ):
        try:
            self.pushNonBreakableOr( token.isOutfixer() )
            try:
                # print( 'TOKEN', token, token.category() )
                minip = self._postfix_table[ token.category() ]
            except KeyError:
                raise Mishap( f'Unexpected token in infix/postfix position', token=token )
            return minip( self, prec, lhs, token, source )
        finally:
            self.popNonBreakable()

    def readExpr( self, prec, source ):
        e = self.tryReadExpr( prec, source )
        if e:
            return e
        elif source.isEmpty():
            raise Mishap( 'Unexpected end of input' )
        else:
            raise Mishap( f'No continuation because of unexpected token', token=source.pop().value())

    def tryReadExpr( self, prec, source, checkNewlines=True ):
        token = source.popOrElse()
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
            # print( 'PEEK', token, token and token.isPostfixer() )
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
        return self.parseFromString( file_object.read() )

    def parseFromString( self, text ):
        source = PeekablePushable( tokenizer( text ) )
        yield from self.readStatementsGenerator( source )
        if not source.isEmpty():
            raise Mishap( 'Unexpected token after end of statements', token=source.peekOrElse() )

def mustRead( source, *categories ):
    token = source.popOrElse()
    if token:
        if token.category() not in categories:
            raise Mishap( f'Required keyword not found', found=token, wanted=str(categories) )
    else:
        raise Mishap( 'Unexpected end of file' )

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
        mustRead( source, "RPAREN" )
        return e

def lbracketPrefixMiniParser( parser, token, source ):
    if tryRead( source, 'RBRACKET' ):
        kernel = codetree.SeqCodelet()
    else:
        kernel = parser.readExpr( math.inf, source )
        mustRead( source, "RBRACKET" )
    return codetree.SyscallCodelet( name="newImmutableList", arguments=kernel )

def defPrefixMiniParser( parser, token, source ):
    funcArgs = parser.readFuncArgs( source )
    funcArgs.declarationMode()
    mustRead( source, 'END_PARAMETERS', 'END_PHRASE' )
    b = parser.readStatements( source )
    mustRead( source, 'END_DEC_FUNCTION_1', 'END' )
    func = funcArgs.function()
    args = funcArgs.arguments()
    id = codetree.IdCodelet( name=func.name(), reftype="val" )
    func = codetree.LambdaCodelet( parameters=args, body=b )
    return codetree.BindingCodelet( lhs=id, rhs=func )

def forPrefixMiniParser( parser, token, source ):
    # for ^ QUERY do STMNTS endfor
    query = parser.readExpr( math.inf, source )
    # for QUERY ^ do STMNTS endfor
    mustRead( source, 'DO', 'END_PHRASE' )
    # for QUERY do ^ STMNTS endfor
    body = parser.readStatements( source )
    mustRead( source, "ENDFOR", "END" )
    return codetree.ForCodelet( query=query, body=body )

def ifPrefixMiniParser( parser, token, source ):
    # if ^ EXPR then STMNTS ... endif
    testPart = parser.readExpr( math.inf, source )
    # if EXPR ^ then STMNTS ... endif
    mustRead( source, "THEN", 'END_PHRASE' )
    # if EXPR then ^ STMNTS ... endif
    thenPart = parser.readStatements( source )
    # if EXPR then STMNTS ^ (elseif EXPR then STATEMENTS ... | else STMNTS | )  endif
    if tryRead( source, "ELSE_IF" ):
        # if EXPR then STMNTS elseif ^ EXPR then STATEMENTS ... endif
        elsePart = ifPrefixMiniParser( parser, None, source )
        # if EXPR then STMNTS elseif EXPR then STATEMENTS .... endif ^
        return codetree.IfCodelet( testPart=testPart, thenPart=thenPart, elsePart=elsePart )
    elif tryRead( source, "ELSE" ):
        tryRead( source, 'END_PHRASE' )                 ### Discard optional colon
        # if EXPR then STMNTS else ^ STMNTS endif
        elsePart = parser.readStatements( source )
        # if EXPR then STMNTS else STMNTS ^ endif
        mustRead( source, "END_IF" )
        # if EXPR then STMNTS else STMNTS endif ^
        return codetree.IfCodelet( testPart=testPart, thenPart=thenPart, elsePart=elsePart )
    else:
        # if EXPR then STMNTS ^ endif
        mustRead( source, "END_IF", "END" )
        # if EXPR then STMNTS endif ^
        return codetree.IfCodelet( testPart=testPart, thenPart=thenPart, elsePart=codetree.SeqCodelet() )

def varPrefixMiniParser( parser, token, source ):
    t = source.pop()
    if t.category() == IdToken:
        return codetree.IdCodelet( name=t.value(), reftype=token.value() )
    else:
        raise Exception( f"Unexpected token: {t}")


PREFIX_TABLE = {
    "LPAREN": lparenPrefixMiniParser,
    "LBRACKET": lbracketPrefixMiniParser,
    "DEC_FUNCTION_1": defPrefixMiniParser,
    "IF": ifPrefixMiniParser,
    "FOR": forPrefixMiniParser,
    BasicToken: lambda parser, token, source: codetree.StringCodelet( value=token.value() ),
    IdToken: lambda parser, token, source: codetree.IdCodelet( name=token.value(), reftype="get" ),
    IntToken: lambda parser, token, source: codetree.IntCodelet( value=token.value() ),
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
        mustRead( source, "RPAREN" )
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


POSTFIX_TABLE = {
    "SEQ": commaPostfixMiniParser,
    "LPAREN": lparenPostfixMiniParser,
    IdToken: idPostfixMiniParser,
    "BIND": bindPostfixMiniParser,
    "ASSIGN": assignPostfixMiniParser,
    "IN": inPostfixMiniParser,
}

def standardParser():
    return TableDrivenParser( PREFIX_TABLE, POSTFIX_TABLE )

def parseFromString( text ):
    return standardParser().parseFromString( text )

def parseFromFileObject( file_object ):
    return standardParser().parseFromFileObject( file_object )

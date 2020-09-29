"""
parser -- parser module for the Nutmeg compiler
"""

import codetree
import tokens
from tokenizer import tokenizer
from peekablepushable import PeekablePushable
import math

class TableDrivenParser:
    """
    Table-driven recursive descent parser
    """

    def __init__( self, prefix_table, postfix_table ):
        self._prefix_table = prefix_table
        self._postfix_table = postfix_table

    def tryRunPrefixMiniParser( self, token, source ):
        try:
            minip = self._prefix_table[ token.category() ]
            return minip( self, token, source )
        except KeyError:
            return None

    def runPostfixMiniParser( self, prec, lhs, token, source ):
        try:
            minip = self._postfix_table[ token.category() ]
            return minip( self, prec, lhs, token, source )
        except KeyError:
            raise Exception( f'Unexpected token in infix/postfix position {token}')

    def readExpr( self, prec, source ):
        e = self.tryReadExpr( prec, source )
        if e:
            return e
        elif source.isEmpty():
            raise Exception( 'Unexpected end of input' )
        else:
            raise Exception( f'Unexpected token {source.pop()}')

    def tryReadExpr( self, prec, source ):
        token = source.popOrElse()
        if not token:
            return None
        sofar = self.tryRunPrefixMiniParser( token, source )
        if not sofar:
            return None
        while True:
            token = source.peekOrElse()
            # print( 'PEEK', token, token and token.isPostfixer() )
            if not token or not token.isPostfixer(): break
            p = token.precedence()
            # print( 'PREC', p )
            if not p or not p <= prec: break
            source.pop()
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
        args = expr.arguments()
        return TableDrivenParser.isntArgs( expr.arguments() )

    def readFuncArgs( self, source ):
        funcargs = self.readExpr( math.inf, source )
        issue = TableDrivenParser.isntFuncArgs( funcargs )
        if issue:
            raise Exception( f'Invalid expression for function definition ({issue})' )
        else:
            return funcargs

    def readStatements( self, source ):
        body = []
        while not source.isEmpty():
            e = self.tryReadExpr( math.inf, source )
            if e:
                body.append( e )
            else:
                break
            if not self.tryReadExpr( source, 'TERMINATE_STATEMENT' ):
                break
        return codetree.SeqCodelet( body=body )

    def parseFromFileObject(self, file_object):
        return self.parseFromString( file_object.read() )

    def parseFromString( self, text ):
        source = PeekablePushable( tokenizer( text ) )
        while not source.isEmpty():
            yield self.readExpr( math.inf, source )


################################################################################
### Set up the tables
################################################################################

def mustRead( source, *categories ):
    token = source.popOrElse()
    if token:
        if token.category() not in categories:
            raise Exception( f'Unexpected token: {token}, wanted {str(categories)}' )
    else:
        raise Exception( 'Unexpected end of file' )

def tryRead( source, *categories ):
    token = source.peekOrElse()
    ok = token and token.category() in categories
    if ok:
        source.pop()
    return ok

def lparenPrefixMiniParser( parser, token, source ):
    if tryRead( source, 'RPAREN' ):
        return codetree.SeqCodelet()
    else:
        e = parser.readExpr( math.inf, source )
        mustRead( source, "RPAREN" )
        return e


def defPrefixMiniParser( parser, token, source ):
    funcArgs = parser.readFuncArgs( source )
    funcArgs.declarationMode()
    mustRead( source, 'END_PARAMETERS', 'END_PHRASE' )
    b = parser.readExpr( math.inf, source )           # Should be statements, not an expr.
    mustRead( source, 'END_DEC_FUNCTION_1', 'END' )
    # TODO: horribly wrong!
    func = funcArgs.function()
    args = funcArgs.arguments()
    id = codetree.IdCodelet( name=func.name(), reftype="val" )
    func = codetree.FunctionCodelet( parameters=args, body=b)
    return codetree.BindingCodelet( lhs=id, rhs=func )

PREFIX_TABLE = {
    "LPAREN": lparenPrefixMiniParser,
    "DEC_FUNCTION_1": defPrefixMiniParser,
    tokens.BasicToken: lambda parser, token, source: codetree.StringCodelet( value=token.value() ),
    tokens.IdToken: lambda parser, token, source: codetree.IdCodelet( name=token.value(), reftype="get" ),
    tokens.IntToken: lambda parser, token, source: codetree.IntCodelet( value=token.value() )
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

POSTFIX_TABLE = {
    "SEQ": commaPostfixMiniParser,
    "LPAREN": lparenPostfixMiniParser,
    tokens.IdToken: idPostfixMiniParser
}

def standardParser():
    return TableDrivenParser( PREFIX_TABLE, POSTFIX_TABLE )

def parseFromString( text ):
    return standardParser().parseFromString( text )

def parseFromFileObject( file_object ):
    return standardParser().parseFromFileObject( file_object )

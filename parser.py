"""
parser -- parser module for the Nutmeg compiler
"""

import codetree
import tokens
from tokenizer import tokenizer
from peekablepushable import PeekablePushable

class TableDrivenParser:
    """
    Table-driven recursive descent parser
    """

    def __init__( self, prefix_table, postfix_table ):
        self._prefix_table = prefix_table
        self._postfix_table = postfix_table

    def runPrefixMiniParser( self, token, source ):
        try:
            minip = self._prefix_table[ token.category() ]
            return minip( self, token, source )
        except KeyError:
            return token.toCodeTree()

    def runPostfixMiniParser( self, prec, lhs, token, source ):
        try:
            minip = self._postfix_table[ token.category() ]
            return minip( self, prec, lhs, token, source )
        except KeyError:
            raise Exception( 'Dunno what to do with this one' )

    def readExpr( self, prec, source ):
        token = source.peekOrElse()
        if token:
            next( source )
            sofar = self.runPrefixMiniParser( token, source )
            while True:
                token = source.peekOrElse()
                if not token or not token.isPostfixer(): break
                p = token.precedence()
                if not p <= prec: break
                next(source)
                sofar = self.runPostfixMiniParser( p, sofar, token, source )
            return sofar
        else:
            raise Exception( 'Unexpected end of input' )

    def parseFromFileObject(self, file_object):
        return self.parseFromString( file_object.read() )

    def parseFromString( self, text ):
        source = PeekablePushable( tokenizer( text ) )
        while not source.isEmpty():
            yield self.readExpr( float('inf'), source )


################################################################################
### Set up the tables
################################################################################

PREFIX_TABLE = {
    tokens.BasicToken: lambda parser, token, source: codetree.StringCodelet( value=token.value() ),
    tokens.IdToken: lambda parser, token, source: codetree.IdCodelet( name=token.value(), reftype="get" ),
    tokens.IntToken: lambda parser, token, source: codetree.IntCodelet( value=token.value() ),
}

def idPostfixMiniParser( parser, p, lhs, token, source ):
    rhs = parser.readExpr( p, source )
    return codetree.SyscallCodelet(
        name=token.value(),
        arguments=codetree.SeqCodelet( lhs, rhs )
    )

POSTFIX_TABLE = {
    tokens.IdToken: idPostfixMiniParser
}

def standardParser():
    return TableDrivenParser( PREFIX_TABLE, POSTFIX_TABLE )

def parseFromString( text ):
    return standardParser().parseFromString( text )

def parseFromFileObject( file_object ):
    return standardParser().parseFromFileObject( file_object )

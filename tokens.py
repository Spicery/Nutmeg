# token_spec is the specification for Nutmeg's tokens
#
# token_spec is used by Tokenizer to generate a stream of tokens in form
# e.g. Token(type="int", value="42")
#
# Each key is a category of the syntax: "literal_constants", "operators" etc.
# Categories are for ease of human reading only: they have no functional utility
# and can be changed.
#
# Each value is a dictionary of the token type and its matching regex
# pattern: { <*token type*>: r"*matching regex pattern*" }.
#
# The named capturing group within each regex pattern (e.g. "?P<int>")
# is required for Tokenizer to match regex pattern with token type.

import re
import abc
import codetree

class Token( abc.ABC ):

    def __init__( self, value ):
        self._value = value

    def __str__( self ):
        return f'<{type(self).__name__} {self._value}>'

    def __eq__( self, other ):
        return isinstance( other, type(self) ) and self._value == other._value

    def precedence( self ):
        return 0

    def value( self ):
        return self._value

    @abc.abstractmethod
    def category( self ):
        pass

    def isPrefixer( self ):
        return True

    def isPostfixer( self ):
        return False


class BasicToken( Token ):
    """
    TODO: scaffolding class
    """

    def  __init__( self, value, toktype ):
        super().__init__( value )
        self._toktype = toktype

    def __eq__( self, other ):
        return isinstance( other, type(self) ) and self._value == other._value and self._toktype == other._toktype

    def category( self ):
        return type(self)

class IdToken( Token ):

    def  __init__( self, value, prec ):
        super().__init__( value )
        self._prec = prec

    def __eq__( self, other ):
        # TODO: Remove after sorting out the tests
        return isinstance( other, type(self) ) and self._value == other._prec and self._toktype == other._prec

    @staticmethod
    def make( toktype, match ):
        return IdToken( match.group( match.lastgroup ), toktype.precedence() )

    def precedence( self ):
        return self._prec

    def isPostfixer( self ):
        return self._prec > 0

    def category( self ):
        return type(self)

    def toCodeTree( self ):
        return codetree.IdCodelet( name=self._value, reftype="get" )

class IntToken( Token ):

    @staticmethod
    def make( toktype, match ):
        return IntToken( match.group( match.lastgroup ) )

    def category( self ):
        return type(self)

class TokenType:

    def __init__( self, regex_str, prec = 0, make = None ):
        self._regex_str = regex_str
        self._make = make
        self._prec = prec
        m = re.match( r'(?:(?!\(\?P<).)*\(\?P<(\w+)>', self._regex_str )
        if m:
            self._idname = m.group( 1 )
        else:
            raise Exception( f'Invalid token specification: {self._regex_str}' )

    def idname( self ):
        return self._idname

    def regex_str( self ):
        return self._regex_str

    def precedence( self ):
        return self._prec

    def newToken( self, match ):
        if self._make:
            return self._make( self, match )
        else:
            # TODO: this needs stripping out
            return BasicToken( match.group( match.lastgroup ), match.lastgroup )

token_spec = {
    tt.idname(): tt for tt in [
        # identifier
        TokenType( r"(?i)(?P<ID>([a-z_])\w*)", make=IdToken.make ),

        # literal_constants
        TokenType( r"(?P<INT>(\+|-)?\d+)", make=IntToken.make ),
        TokenType( r'(?P<S_STRING>(?!""")"[^\n"]*")' ),
        TokenType( r"(?P<DQSTRING>(?!''')'[^\n']*')" ),
        TokenType( r'(?P<SQSTRING>"""(?:(?!""").)*""")' ),
        TokenType( r'(?P<MULTILINE_DQSTRING>"""(?:(?!""").)*""")' ),
        TokenType( r"(?P<MULTILINE_SQSTRING>'''(?:(?!''').)*''')" ),
        TokenType( r"(?P<WS>\s+)" ),

        # operators
        TokenType( r"(?P<BIND>:=)" ),
        TokenType( r"(?P<ASSIGN><-)" ),
        TokenType( r"(?P<UPDATE_ELEMENT><--)" ),
        TokenType( r"(?P<COPY_AND_SET><==)" ),
        TokenType( r"(?P<PLUS>\+)", prec=100, make=IdToken.make ),
        TokenType( r"(?P<MINUS>-)" ),
        TokenType( r"(?P<TIMES>\*)" ),
        TokenType( r"(?P<DIVIDE>/)" ),

        # separators
        TokenType( r"(?P<TERMINATE_STATEMENT>;)" ),
        TokenType( r"(?P<LPAREN>\()" ),
        TokenType( r"(?P<RPAREN>\))" ),

        # keywords"
        TokenType( r"(?P<DEC_VARIABLE>var)" ),
        TokenType( r"(?P<DEC_NONASSIGNABLE>val)" ),
        TokenType( r"(?P<DEC_IMMUTABLE>const)" ),
        TokenType( r"(?P<DEC_FUNCTION_1>def)" ),
        TokenType( r"(?P<DEC_FUNCTION_2>function)" ),

        # comments
        TokenType( r"(?P<COMMENT_LINE>###)[^\n]*\n" ),
        TokenType( r"(?P<COMMENT_BLOCK_START>\#\#\()" ),

    ]
}


token_spec_regex = re.compile( "|".join( [ tt.regex_str() for tt in token_spec.values() ] ), re.DOTALL )

# Multi-line comments are a bit more complicated if you want OK error messages.
comment_block_start = re.compile( token_spec['COMMENT_BLOCK_START'].regex_str(), re.DOTALL )
comment_block_middle = re.compile( r"((?!##[()]).)+", re.DOTALL)
comment_block_end = re.compile( r"(?P<COMMENT_BLOCK_FINISH>\#\#\))", re.DOTALL )


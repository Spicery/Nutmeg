import re
import abc

class Token( abc.ABC ):

    def __init__( self, value ):
        self._value = value

    def __eq__( self, other ):
        """TODO: Only needed for unit tests - must be a better way"""
        return isinstance( other, type(self) ) and self._value == other._value

    def __str__( self ):
        return f'<{type( self ).__name__} {self._value}>'

    def precedence( self ):
        return None

    @abc.abstractmethod
    def category( self ):
        pass

    def value( self ):
        return self._value

    def isPrefixer( self ):
        return True

    def isPostfixer( self ):
        return False

    def checkCategory( self, c ):
        if c != self.category():
            raise Exception( f'Unexpected token in input: {self.value()}' )


class BasicToken( Token ):
    """
    TODO: scaffolding class, remove later.
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

    def category( self ):
        return type(self)

    def isPostfixer( self ):
        return self._prec


class IntToken( Token ):

    @staticmethod
    def make( toktype, match ):
        return IntToken( match.group( match.lastgroup ) )

    def category( self ):
        return type(self)

class PunctuationToken( Token ):

    def  __init__( self, value, idname ):
        super().__init__( value )
        self._idname = idname

    @staticmethod
    def make( tokentype, match ):
        return PunctuationToken( match.group( match.lastgroup ), tokentype.idname() )

    def isPrefixer( self ):
        return False

    def category( self ):
        return self._idname


class SyntaxToken( Token ):

    def  __init__( self, value, prec, prefix, idname ):
        super().__init__( value )
        self._prec = prec
        self._prefix = prefix
        self._idname = idname

    @staticmethod
    def make( toktype, match ):
        return SyntaxToken( match.group( match.lastgroup ), toktype.precedence(), toktype.prefix(), toktype.idname() )

    def __eq__( self, other ):
        # TODO: Remove after sorting out the tests
        return (
            isinstance( other, type(self) ) and
            self._prec == other._prec and
            self._prefix == other._prefix
        )

    def precedence( self ):
        return self._prec

    def category( self ):
        return self._idname

    def isPostfixer( self ):
        return self._prec > 0

    def isPrefixer( self ):
        return self._prefix

class TokenType:

    def __init__( self, regex_str, make = None, prec = 0, prefix = True ):
        self._regex_str = regex_str
        self._make = make
        self._prec = prec
        self._prefix = prefix
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

    def prefix( self ):
        return self._prefix

    def newToken( self, match ):
        if self._make:
            return self._make( self, match )
        else:
            # TODO: this needs stripping out
            return BasicToken( match.group( match.lastgroup ), match.lastgroup )

token_spec = {
    tt.idname(): tt for tt in [
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
        TokenType( r"(?P<MINUS>-)", prec=100, make=IdToken.make ),
        TokenType( r"(?P<TIMES>\*)", prec=90, make=IdToken.make ),
        TokenType( r"(?P<DIVIDE>/)", prec=90, make=IdToken.make ),
        TokenType( r"(?P<SEQ>,)", prec=1000, prefix=False, make=SyntaxToken.make ),

        # separators
        TokenType( r"(?P<TERMINATE_STATEMENT>;)", make=PunctuationToken.make ),
        TokenType( r"(?P<END_PHRASE>:)", make=PunctuationToken.make ),
        TokenType( r"(?P<END_PARAMETERS>=>>)", make=PunctuationToken.make ),
        TokenType( r"(?P<LPAREN>\()", prec=10, prefix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<RPAREN>\))", make=PunctuationToken.make ),
        TokenType( r"(?P<END_DEC_FUNCTION_1>enddef)", make=SyntaxToken.make ),
        TokenType( r"(?P<END_DEC_FUNCTION_2>endfunction)", make=SyntaxToken.make ),
        TokenType( r"(?P<END>end)", make=PunctuationToken.make ),                           # MUST come after all other end... token types.

        # keywords"
        TokenType( r"(?P<DEC_VARIABLE>var)" ),
        TokenType( r"(?P<DEC_NONASSIGNABLE>val)" ),
        TokenType( r"(?P<DEC_IMMUTABLE>const)" ),
        TokenType( r"(?P<DEC_FUNCTION_1>def)", make=SyntaxToken.make ),
        TokenType( r"(?P<DEC_FUNCTION_2>function)" ),

        # comments
        TokenType( r"(?P<COMMENT_LINE>###)[^\n]*\n" ),
        TokenType( r"(?P<COMMENT_BLOCK_START>\#\#\()" ),

        # identifier
        TokenType( r"(?i)(?P<ID>([a-z_])\w*)", make=IdToken.make ),                         # Must come after all other keywords.
    ]
}


token_spec_regex = re.compile( "|".join( [ tt.regex_str() for tt in token_spec.values() ] ), re.DOTALL )

# Multi-line comments are a bit more complicated if you want OK error messages.
comment_block_start = re.compile( token_spec['COMMENT_BLOCK_START'].regex_str(), re.DOTALL )
comment_block_middle = re.compile( r"((?!##[()]).)+", re.DOTALL)
comment_block_end = re.compile( r"(?P<COMMENT_BLOCK_FINISH>\#\#\))", re.DOTALL )


import re
import abc

def literalStringTranslation( token_text : str ):
    """
    Translate the raw token-text of a string into the string that is denoted
    by the token.
    :param token_text: the raw token text e.g. 'foo'
    :return: the string constant's value e.g. foo
    """
    # Discard the leading/trailing quotes.
    if token_text.startswith( "'''" ) or token_text.startswith( '"""' ):
        token_text = token_text[ 3 : -4 ]
    elif token_text.startswith( "'" ) or token_text.startswith( '"' ):
        token_text = token_text[ 1 : -1 ]
    else:
        raise Exception( f'Internal error during tokenisation - cannot understand this string token: {token_text}')
    # TODO - decode any escape sequences.
    return token_text

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

    def valueForLiteralString( self ):
        return literalStringTranslation( self._value )

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
        return isinstance( other, type(self) ) and self._value == other._value and self._prec == other._prec

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

class BoolToken( Token ):

    @staticmethod
    def make( toktype, match ):
        return BoolToken( match.group( match.lastgroup ) )

    def category( self ):
        return type(self)

class StringToken( Token ):

    @staticmethod
    def make( toktype, match ):
        token_text = match.group( match.lastgroup )
        return StringToken( token_text )

    def literalValue( self ):
        return literalStringTranslation( self._value )

    def category( self ):
        return type( self )


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


################################################################################
### Token table
################################################################################

token_spec = {
    tt.idname(): tt for tt in [
        # literal_constants
        TokenType( r"(?P<INT>(\+|-)?\d+)", make=IntToken.make ),
        TokenType( r'(?P<S_STRING>(?!""")"[^\n"]*")', make=StringToken.make ),
        TokenType( r"(?P<DQSTRING>(?!''')'[^\n']*')", make=StringToken.make ),
        TokenType( r'(?P<SQSTRING>"""(?:(?!""").)*""")', make=StringToken.make ),
        TokenType( r'(?P<MULTILINE_DQSTRING>"""(?:(?!""").)*""")', make=StringToken.make ),
        TokenType( r"(?P<MULTILINE_SQSTRING>'''(?:(?!''').)*''')", make=StringToken.make ),
        TokenType( r"(?P<BOOL>true|false)", make=BoolToken.make ),
        TokenType( r"(?P<WS>\s+)" ),

        # operators
        TokenType( r"(?P<BIND>:=)", prec=990, make=SyntaxToken.make ),
        TokenType( r"(?P<ASSIGN><-)", prec=990, make=SyntaxToken.make ),
        TokenType( r"(?P<UPDATE_ELEMENT><--)" ),
        TokenType( r"(?P<COPY_AND_SET><==)" ),
        TokenType( r"(?P<PLUS>\+)", prec=190, make=IdToken.make ),
        TokenType( r"(?P<MINUS>-)", prec=190, make=IdToken.make ),
        TokenType( r"(?P<TIMES>\*)", prec=180, make=IdToken.make ),
        TokenType( r"(?P<DIVIDE>/)", prec=180, make=IdToken.make ),
        TokenType( r"(?P<LTE><=)", prec=590, make=IdToken.make ),
        TokenType( r"(?P<SEQ>,)", prec=1000, prefix=False, make=SyntaxToken.make ),

        # keywords
        TokenType( r"(?P<TERMINATE_STATEMENT>;)", make=PunctuationToken.make ),
        TokenType( r"(?P<END_PHRASE>:)", make=PunctuationToken.make ),
        TokenType( r"(?P<END_PARAMETERS>=>>)", make=PunctuationToken.make ),
        TokenType( r"(?P<LPAREN>\()", prec=10, prefix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<RPAREN>\))", make=PunctuationToken.make ),
        TokenType( r"(?P<END_DEC_FUNCTION_1>enddef)", make=SyntaxToken.make ),
        TokenType( r"(?P<END_DEC_FUNCTION_2>endfunction)", make=SyntaxToken.make ),
        TokenType( r"(?P<IF>if)", prefix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<THEN>then)", make=PunctuationToken.make ),
        TokenType( r"(?P<ELSE_IF>elseif)", make=PunctuationToken.make ),
        TokenType( r"(?P<ELSE>else)", make=PunctuationToken.make ),
        TokenType( r"(?P<END_IF>endif)", make=PunctuationToken.make ),
        TokenType( r"(?P<END>end)", make=PunctuationToken.make ),                           # MUST come after all other end... token types.

        # keywords"
        TokenType( r"(?P<DEC_VARIABLE>var)", make=SyntaxToken.make ),
        TokenType( r"(?P<DEC_NONASSIGNABLE>val)", make=SyntaxToken.make ),
        TokenType( r"(?P<DEC_IMMUTABLE>const)", make=SyntaxToken.make ),
        TokenType( r"(?P<DEC_FUNCTION_1>def)", make=SyntaxToken.make ),
        TokenType( r"(?P<DEC_FUNCTION_2>function)" ),

        # comments
        TokenType( r"(?P<COMMENT_LINE>###)[^\n]*\n" ),
        TokenType( r"(?P<COMMENT_BLOCK_START>\#\#\()" ),

        # identifier
        TokenType( r"(?P<ID>([A-Za-z_])\w*)", make=IdToken.make ),                         # Must come after all other keywords.
    ]
}


token_spec_regex = re.compile( "|".join( [ tt.regex_str() for tt in token_spec.values() ] ), re.DOTALL )

# Multi-line comments are a bit more complicated if you want OK error messages.
comment_block_start = re.compile( token_spec['COMMENT_BLOCK_START'].regex_str(), re.DOTALL )
comment_block_middle = re.compile( r"((?!##[()]).)+", re.DOTALL)
comment_block_end = re.compile( r"(?P<COMMENT_BLOCK_FINISH>\#\#\))", re.DOTALL )


################################################################################
### tokenizer itself
################################################################################

def scan_nested_comment( text, position ):
    """We do something special to handle multi-line comments."""
    # We start having snipped off an opening long-comment, so depth = 1.
    depth = 1
    while depth > 0:
        while True:
            m = comment_block_middle.match( text, position )
            if not m:
                break
            position = m.end()
        m = comment_block_end.match( text, position )
        if m:
            position = m.end()
            depth -= 1
        else:
            m = comment_block_start.match( text, position )
            if m:
                position = m.end()
                depth += 1
            else:
                raise Exception( 'Multi-line comment not terminated properly' )
    return position

def tokenizer( text : str ):
    """
    Simple scanner for working on input supplied as a string.
    """
    position = 0
    while position < len( text ):
        m = token_spec_regex.match( text, position )
        if m:
            idname = m.lastgroup
            position = m.end()
            if idname == "COMMENT_BLOCK_START":
                position = scan_nested_comment( text, position )
            elif idname != "WS" and idname != "COMMENT_LINE":
                token_type = token_spec[idname]
                yield token_type.newToken( m )
        else:
            n = text.find("\n", position)
            msg = text[position:n] if n != -1 else text
            raise Exception( f'Cannot tokenise past this point: {msg}')

if __name__ == "__main__":
    # This is some ad hoc test code.
    for t in tokenizer(
"""
So lets try to "tokenise" this ### With an end of line comment.
Then a 
    ''' same here
    Multi-line string
    With multiple lines! 
    ''' some more context
And finally some 99 
##( 
    Nested comments that cross a few lines
    And some more here 
##)
""" ): print( t )

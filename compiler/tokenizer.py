import re
import abc
from mishap import Mishap

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

def positionInSourceText( source_text, span ):
    ( start, finish ) = span
    lineCount = 1
    countSinceLastNewline = 0
    countAllChars = 0
    for ch in source_text:
        if countAllChars >= start:
            break
        countAllChars += 1
        countSinceLastNewline += 1
        if ch == '\n':
            lineCount += 1
            countSinceLastNewline = 0
    cxt = createContext( countSinceLastNewline, source_text, start )
    # Limit the context to 60 characters, inserting '...' if either end is truncated
    return f"line:{lineCount}, character:{countSinceLastNewline+1}, context:{cxt}"

CONTEXT_RADIUS = 30
def createContext( countSinceLastNewline, source_text, start ):
    startOfCurrentLine = start - countSinceLastNewline
    endOfCurrentLine = source_text.find( "\n", startOfCurrentLine )
    line = source_text[ startOfCurrentLine: endOfCurrentLine ]
    # Grab the surrounding context
    low = max( countSinceLastNewline - CONTEXT_RADIUS, 0 )
    cxt_lhs = line[ low:countSinceLastNewline ]
    if low != 0:
        cxt_lhs = '...' + cxt_lhs
    high = min( countSinceLastNewline + CONTEXT_RADIUS, len( line ) )
    cxt_rhs = line[ countSinceLastNewline:high ]
    if high != len( line ):
        cxt_rhs = cxt_rhs + '...'
    # Insert '^' to indicate current position.
    cxt = cxt_lhs + "^" + cxt_rhs
    return cxt


class Token( abc.ABC ):

    def __init__( self, value ):
        self._value = value
        self._followsNewLine = False
        self._span = None
        self._sourceText = None

    def positionInText( self ):
        return positionInSourceText( self._sourceText, self._span )

    def span( self ):
        return self._span

    def setSpan( self, start, finish ):
        self._span = ( start, finish )

    def setSourceText( self, txt ):
        self._sourceText = txt

    def followsNewLine( self ):
        return self._followsNewLine

    def setFollowsNewLine( self ):
        self._followsNewLine = True

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

    def isPrefixerOnly( self ):
        return self.isPrefixer() and not self.isPostfixer()

    def isOutfixer( self ):
        return False

    def isPostfixer( self ):
        return False

    def isPostfixerOnly( self ):
        return not self.isPrefixer() and self.isPostfixer()

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

    def valueAsInt( self ):
        return int( self._value, base=0 )

class CharToken( Token ):

    @staticmethod
    def make( toktype, match ):
        return CharToken( match.group( match.lastgroup ) )

    def literalValue( self ):
        return self._value[1]

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

    def  __init__( self, value, prec, prefix, outfix, idname ):
        super().__init__( value )
        self._prec = prec
        self._prefix = prefix
        self._outfix = outfix
        self._idname = idname

    @staticmethod
    def make( toktype, match ):
        return SyntaxToken( match.group( match.lastgroup ), toktype.precedence(), toktype.prefix(), toktype.outfix(), toktype.idname() )

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

    def isOutfixer( self ):
        return self._outfix

class TokenType:

    def __init__( self, regex_str, make = None, prec = 0, prefix = True, outfix = False ):
        self._regex_str = regex_str
        self._make = make
        self._prec = prec
        self._prefix = prefix or outfix
        self._outfix = outfix
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

    def outfix( self ):
        return self._outfix

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
        TokenType( r"(?P<HEXINT>(\+|-)?0x_?[\dA-F]+(?:_[A-F\d]+)*)", make=IntToken.make ),
        TokenType( r"(?P<BOOLINT>(\+|-)?0b_?[01]+(?:_[01_]+)*)", make=IntToken.make ),
        TokenType( r"(?P<INT>(\+|-)?[1-9]\d*(?:_\d+)*)", make=IntToken.make ),
        TokenType( r"(?P<ZEROINT>(\+|-)?0)", make=IntToken.make ),
        TokenType( r"(?P<CHAR>\`[^\n\`]\`)", make=CharToken.make ),
        TokenType( r'(?P<DQSTRING>(?!""")"[^\n"]*")', make=StringToken.make ),
        TokenType( r"(?P<SQSTRING>(?!''')'[^\n']*')", make=StringToken.make ),
        TokenType( r'(?P<MULTILINE_DQSTRING>"""(?:(?!""").)*""")', make=StringToken.make ),
        TokenType( r"(?P<MULTILINE_SQSTRING>'''(?:(?!''').)*''')", make=StringToken.make ),
        TokenType( r"(?P<BOOL>true|false)", make=BoolToken.make ),
        TokenType( r"(?P<WS>\s+)" ),

        # operators
        TokenType( r"(?P<ANNOTATION>@)", make=SyntaxToken.make ),
        TokenType( r"(?P<BIND>:=)", prec=990, make=SyntaxToken.make ),
        TokenType( r"(?P<ASSIGN><-)", prec=990, make=SyntaxToken.make ),
        TokenType( r"(?P<UPDATE_ELEMENT><--)" ),
        TokenType( r"(?P<COPY_AND_SET><==)" ),
        TokenType( r"(?P<PLUSPLUS>\++)", prec=500, make=IdToken.make ),
        TokenType( r"(?P<PLUS>\+)", prec=190, make=IdToken.make ),
        TokenType( r"(?P<MINUS>-)", prec=190, make=IdToken.make ),
        TokenType( r"(?P<TIMES>\*)", prec=180, make=IdToken.make ),
        TokenType( r"(?P<DIVIDE>/)", prec=180, make=IdToken.make ),
        TokenType( r"(?P<HALF_OPEN_INTERVAL>\.\.<)", prec=240, make=IdToken.make ),
        TokenType( r"(?P<CLOSED_INTERVAL>\.\.\.)", prec=240, make=IdToken.make ),
        TokenType( r"(?P<SEQ>,)", prec=1000, prefix=False, make=SyntaxToken.make ),
        TokenType( r"(?P<DEREF>!)", prec=8, prefix=False, make=SyntaxToken.make ),
        
        # Warning! Short tokens must come AFTER long ones.
        TokenType( r"(?P<LTE><=)", prec=570, make=IdToken.make ),
        TokenType( r"(?P<GTE>>=)", prec=570, make=IdToken.make ),
        TokenType( r"(?P<LT>\<)", prec=570, make=IdToken.make ),
        TokenType( r"(?P<GT>\>)", prec=570, make=IdToken.make ),
        TokenType( r"(?P<EQUALS>==)", prec=580, make=IdToken.make ),
        TokenType( r"(?P<NOT_EQUALS>!=)", prec=580, make=IdToken.make ),
        TokenType( r"(?P<AND>\band\b)", prec=590, prefix=False, outfix=False, make=SyntaxToken.make ),
        TokenType( r"(?P<OR>\bor\b)", prec=595, prefix=False, outfix=False, make=SyntaxToken.make ),

        # keywords
        TokenType( r"(?P<TERMINATE_STATEMENT>;)", make=PunctuationToken.make ),
        TokenType( r"(?P<END_PHRASE>:)", make=PunctuationToken.make ),
        TokenType( r"(?P<END_PARAMETERS>=>>)", make=PunctuationToken.make ),
        TokenType( r"(?P<DOT>\.)", prec=11, make=SyntaxToken.make ),
        TokenType( r"(?P<LPAREN>\()", prec=10, outfix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<RPAREN>\))", make=PunctuationToken.make ),
        TokenType( r"(?P<LBRACKET>\[)", outfix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<RBRACKET>\])", make=PunctuationToken.make ),
        TokenType( r"(?P<LAMBDA>lambda\b)", outfix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<END_LAMBDA>endlambda\b)", make=PunctuationToken.make ),
        TokenType( r"(?P<END_DEC_FUNCTION_1>enddef\b)", make=SyntaxToken.make ),
        TokenType( r"(?P<END_DEC_FUNCTION_2>endfunction\b)", make=SyntaxToken.make ),
        TokenType( r"(?P<IFNOT>ifnot\b)", outfix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<IF>if\b)", outfix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<THEN>then\b)", make=PunctuationToken.make ),
        TokenType( r"(?P<ELSE_IF>elseif\b)", make=PunctuationToken.make ),
        TokenType( r"(?P<ELSE_IFNOT>elseifnot\b)", make=PunctuationToken.make ),
        TokenType( r"(?P<ELSE>else\b)", make=PunctuationToken.make ),
        TokenType( r"(?P<END_IF>endif\b)", make=PunctuationToken.make ),
        TokenType( r"(?P<END_IFNOT>endifnot\b)", make=PunctuationToken.make ),
        TokenType( r"(?P<FOR>for\b)", outfix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<IN>in\b)", prec=910, prefix=False, outfix=False, make=SyntaxToken.make ),
        TokenType( r"(?P<UNTIL>until\b)", prec=910, prefix=True, outfix=False, make=SyntaxToken.make ),
        TokenType( r"(?P<WHILE>while\b)", prec=910, prefix=True, outfix=False, make=SyntaxToken.make ),
        TokenType( r"(?P<AFTERWARDS>afterwards\b)", prec=910, prefix=False, outfix=False, make=SyntaxToken.make ),
        TokenType( r"(?P<DO>do\b)", make=PunctuationToken.make ),
        TokenType( r"(?P<ENDFOR>endfor\b)", make=PunctuationToken.make ),
        TokenType( r"(?P<FN>fn\b)", outfix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<ENDFN>endfn\b)", make=PunctuationToken.make ),

        TokenType( r"(?P<ASSERT>assert\b)", make=SyntaxToken.make ),

        TokenType( r"(?P<END>end\b)", make=PunctuationToken.make ),  # MUST come after all other end... token types.

        # keywords"
        TokenType( r"(?P<DEC_VARIABLE>var\b)", make=SyntaxToken.make ),
        TokenType( r"(?P<DEC_NONASSIGNABLE>val\b)", make=SyntaxToken.make ),
        TokenType( r"(?P<DEC_IMMUTABLE>const\b)", make=SyntaxToken.make ),
        TokenType( r"(?P<DEC_FUNCTION_1>def\b)", outfix=True, make=SyntaxToken.make ),
        TokenType( r"(?P<DEC_FUNCTION_2>function\b)", outfix=True, make=SyntaxToken.make  ),

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
                raise Mishap( 'A multi-line comment is not terminated properly' )
    return position

def tokenizer( text : str ):
    """
    Simple scanner for working on input supplied as a string.
    """
    follows_newline = False
    position = 0
    while position < len( text ):
        m = token_spec_regex.match( text, position )
        if m:
            idname = m.lastgroup
            old_position = position
            position = m.end()
            follows_newline |= text.find( "\n", old_position, position ) != -1
            if idname == "COMMENT_BLOCK_START":
                old_position = position
                position = scan_nested_comment( text, position )
                follows_newline |= text.find( "\n", old_position, position ) != -1
            elif idname != "WS" and idname != "COMMENT_LINE":
                token_type = token_spec[idname]
                tok:Token = token_type.newToken( m )
                if follows_newline:
                    if not tok.isPostfixerOnly():
                        tok.setFollowsNewLine()
                    follows_newline = False
                tok.setSpan( old_position, position )
                tok.setSourceText( text )
                yield tok
        else:
            n = text.find("\n", position)
            msg = text[position:n] if n != -1 else text
            raise Mishap( f'Tokeniser cannot recognise the text that follows' ).addDetails( text=msg )

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

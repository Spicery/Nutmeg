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

class Token:

    def __init__( self, value ):
        self._value = value

    def __str__( self ):
        return f'<{type(self).__name__} {self._value}>'


class BasicToken( Token ):

    def  __init__( self, value, toktype ):
        super().__init__( value )
        self._toktype = toktype


class TokenType:

    def __init__( self, regex_str, make = None, prec = 0 ):
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

    def newToken( self, match ):
        if self._make:
            self._make( match )
        else:
            return BasicToken( match.group( match.lastgroup ), match.lastgroup )

def indexTokens( *token_types ):
    return { tt.idname(): tt for tt in token_types }

# re.compile(r"(?P<MULTILINE_COMMENT>\#\#\((?:(?!##[()]).)*(?:\g<MULTILINE_COMMENT>(?:(?!##[()]).)*)*\#\#\))", re.DOTALL)

token_spec = {
    tt.idname(): tt for tt in [
        # identifier
        TokenType( r"(?i)(?P<ID>([a-z_])\w*)", 0 ),

        # literal_constants
        TokenType( r"(?P<INT>(\+|-)?\d+)", 0 ),
        TokenType( r'(?P<S_STRING>(?!""")"[^\n"]*")', 0 ),
        TokenType( r"(?P<DQSTRING>(?!''')'[^\n']*')", 0 ),
        TokenType( r'(?P<SQSTRING>"""(?:(?!""").)*""")', 0 ),
        TokenType( r'(?P<MULTILINE_DQSTRING>"""(?:(?!""").)*""")', 0 ),
        TokenType( r"(?P<MULTILINE_SQSTRING>'''(?:(?!''').)*''')", 0 ),
        TokenType( r"(?P<WS>\s+)", 0 ),

        # operators
        TokenType( r"(?P<BIND>:=)", 0 ),
        TokenType( r"(?P<ASSIGN><-)", 0 ),
        TokenType( r"(?P<UPDATE_ELEMENT><--)", 0 ),
        TokenType( r"(?P<COPY_AND_SET><==)", 0 ),
        TokenType( r"(?P<PLUS>\+)", 0 ),
        TokenType( r"(?P<MINUS>-)", 0 ),
        TokenType( r"(?P<TIMES>\*)", 0 ),
        TokenType( r"(?P<DIVIDE>/)", 0 ),

        # separators
        TokenType( r"(?P<TERMINATE_STATEMENT>;)", 0 ),
        TokenType( r"(?P<LPAREN>\()", 0 ),
        TokenType( r"(?P<RPAREN>\))", 0 ),

        # keywords"
        TokenType( r"(?P<DEC_VARIABLE>var)", 0 ),
        TokenType( r"(?P<DEC_NONASSIGNABLE>val)", 0 ),
        TokenType( r"(?P<DEC_IMMUTABLE>const)", 0 ),
        TokenType( r"(?P<DEC_FUNCTION_1>def)", 0 ),
        TokenType( r"(?P<DEC_FUNCTION_2>function)", 0 ),

        # comments
        TokenType( r"(?P<COMMENT_LINE>###)[^\n]*\n", 0 ),
        TokenType( r"(?P<COMMENT_BLOCK_START>\#\#\()", 0 ),

    ]
}




token_spec_regex = re.compile( "|".join( [ tt.regex_str() for tt in token_spec.values() ] ), re.DOTALL )

# Multi-line comments are a bit more complicated if you want OK error messages.
comment_block_start = re.compile( token_spec['COMMENT_BLOCK_START'].regex_str(), re.DOTALL )
comment_block_middle = re.compile( r"((?!##[()]).)+", re.DOTALL)
comment_block_end = re.compile( r"(?P<COMMENT_BLOCK_FINISH>\#\#\))", re.DOTALL )


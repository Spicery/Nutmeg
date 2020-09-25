import re
from collections import namedtuple

from tokens import \
    token_spec, \
    token_spec_regex, \
    comment_block_start, \
    comment_block_middle, \
    comment_block_end

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
            elif idname != "WS":
                token_type = token_spec[idname]
                yield token_type.newToken( m )
        else:
            n = text.find("\n", position)
            msg = text[position:n] if n != -1 else text
            raise Exception( f'Cannot tokenise past this point: {msg}')

if __name__ == "__main__":
    # This is some ad hoc test code.
    for t in scanner(
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
import re
from pathlib import Path

# This is a list of (regex, parser) pairs. The regex is used to recognise
# matching filenames.
PARSER_LIST = []

def NutmegParserExtension( filename_regex_string ):
    r"""
    Introduces a decorator for declaring parser extensions.
    e.g. The decoration @NutmegParserExtension( r'(.*)\.gif' ) would introduce
    a parser extension for GIF image files.
    """
    filename_regex = re.compile( filename_regex_string )
    def registerParser( parser ):
        PARSER_LIST.append( ( filename_regex, parser ) )
        return parser
    return registerParser

def findMatchingParser( pathname ):
    path = Path( pathname )
    fname = path.name
    for ( regex, parser ) in PARSER_LIST:
        m = regex.fullmatch( fname )
        if m:
            return ( m, parser )
    raise Exception( f'No parser found for file: {pathname}' )
    

"""
This is an example parser extension. It is a trivial demo as it sucks in
the whole of a file as a string; it would be more sensible to implement it
as a future string, or at least check the size of the file is small before
unconditionally loading it.

The most interesting design question is whether or not the parser should
return a single codetree _or_ return a codetree iterator. We have chosen the
latter but I expect it is an issue that will surface repeatedly.
"""

from nutmeg_extensions import NutmegParserExtension
from codetree import StringCodelet, IdCodelet, BindingCodelet

@NutmegParserExtension(r'(.*)\.txt$')
def dot_txt_parser( file_object, match ):
    varname = match.group(1)
    contents = file_object.read()
    rhs = StringCodelet( value = contents )
    lhs = IdCodelet( name=varname, reftype='const' )
    yield BindingCodelet( lhs=lhs, rhs=rhs )

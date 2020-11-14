from nutmeg_extensions import NutmegParserExtension
from parser import parseFromFileObject

@NutmegParserExtension(r'(.*)\.nutmeg$')
def dot_txt_parser( file_object, match, unit=None ):
    yield from parseFromFileObject( file_object, unit=unit )

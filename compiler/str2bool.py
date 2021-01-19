"""
This code is a variation on the str2bool package at https://pypi.org/project/str2bool/.
The main difference is that raising an exception is not optional and that the
exception raised is integrated with Nutmeg's own exception handling approach.
"""

from mishap import Mishap

_TRUE_VALUES = { 'yes', 'true', 't', 'y', '1' }
_FALSE_VALUES = { 'no', 'false', 'f', 'n', '0' }

def str2bool( value ):
    value = value.lower()
    if value in _TRUE_VALUES:
        return True
    elif value in _FALSE_VALUES:
        return False
    else:
        raise Mishap( f'Cannot safely convert string to bool', string=value )

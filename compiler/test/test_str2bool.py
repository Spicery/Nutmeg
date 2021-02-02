import unittest
from str2bool import str2bool

def test_str2bool():
    assert True is str2bool( 'True' )
    assert True is str2bool( 'T' )
    assert True is str2bool( 'Yes' )
    assert True is str2bool( '1' )
    assert False is str2bool( 'False' )
    assert False is str2bool( 'F' )
    assert False is str2bool( 'No' )
    assert False is str2bool( '0' )

if __name__ == '__main__':
    unittest.main()

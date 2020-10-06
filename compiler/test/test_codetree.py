import codetree
import io

def test_deserialise():
    # Arrange
    jdata = '{ "kind":"if", "test": {"kind":"bool", "value":"true"}, "then": {"kind":"string", "value":"yes"}, "else": {"kind":"string", "value":"yes"} }'
    # Act
    data = codetree.__deserialize( io.StringIO( jdata ) )
    # Assert
    assert isinstance( data, codetree.IfCodelet )
    assert isinstance( data._test, codetree.BoolCodelet )
    assert isinstance( data._then, codetree.StringCodelet )
    assert isinstance( data._else, codetree.StringCodelet )



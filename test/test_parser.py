from parser import parseFromString
import json

def parseOne( text ):
    e = [ *parseFromString( text ) ]
    assert len( e ) == 1
    return e[0]

def test_parses_identifier():
    assert parseOne( "x" ).toJSON() == {
        "kind": "id",
        "name": "x",
        "reftype": "get",
    }

def test_parses_unsigned_int():
    assert parseOne( "99" ).toJSON() == {
        "kind": "int",
        "value": "99",
    }

def test_parses_signed_pos_int():
    assert parseOne( "+99" ).toJSON() == {
        "kind": "int",
        "value": "99",
    }

def test_parses_signed_neg_int():
    assert parseOne( "-99" ).toJSON() == {
        "kind": "int",
        "value": "-99",
    }

from tabledrivenparser import TableDrivenParser
import json

def test_parses_identifier():
    assert json.loads( TableDrivenParser().parse( "x" ).serialise() ) == {
        "kind": "id",
        "name": "x",
        "reftype": "var",
    }

def test_parses_unsigned_int():
    assert json.loads( TableDrivenParser().parse( "99" ).serialise() ) == {
        "kind": "int",
        "value": 99,
    }


def test_parses_signed_pos_int():
    assert json.loads( TableDrivenParser().parse( "+99" ).serialise() ) == {
        "kind": "int",
        "value": 99,
    }


def test_parses_signed_neg_int():
    assert json.loads( TableDrivenParser().parse( "-99" ).serialise() ) == {
        "kind": "int",
        "value": -99,
    }

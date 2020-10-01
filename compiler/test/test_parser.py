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

################################################################################
### Tests for Issue #14
################################################################################

def test_issue14_1():
    assert parseOne( "x" ).toJSON() == { "kind": "id", "name": "x", "reftype": "get" }

def test_issue14_3():
    parseOne('x + 99').toJSON() == {
        "kind": "syscall", "name": "+",
        "arguments": {
            "kind": "seq", "body": [
                { "kind": "id", "name": "x" },
                { "kind": "int", "value": "99" }
            ]
        }
    }

def test_issue14_4():
    parseOne('x * y + 99').toJSON() == {
        "kind": "syscall",
        "name": "+",
        "arguments": {
            "kind": "seq",
            "body": [
                {
                    "kind": "syscall",
                    "name": "*",
                    "arguments": {
                        "kind": "seq",
                        "body": [
                            {
                                "kind": "id",
                                "name": "x"
                            },
                            {
                                "kind": "id",
                                "name": "y"
                            }
                        ]
                    }
                },
                {
                    "kind": "int",
                    "value": "99"
                }
            ]
        }
    }

def test_issue14_5():
    parseOne( '(xyz)' ).toJSON() == { "kind": "id", "name": "xyz", "reftype": "get" }

def test_issue14_6():
    parseOne( '((99))' ).toJSON() == { "kind": "int", "value": "99" }

def test_issue14_7():
    parseOne( 'x * ( y + 99 )' ).toJSON() == {
        "kind": "syscall",
        "name": "*",
        "arguments": {
            "kind": "seq",
            "body": [
                {
                    "kind": "id",
                    "value": "x",
                    "reftype": "get"
                },
                {
                    "kind": "syscall",
                    "name": "+",
                    "arguments": {
                        "kind": "seq",
                        "body": [
                            {
                                "kind": "id",
                                "name": "y",
                                "reftype": "get"
                            },
                            {
                                "kind": "int",
                                "value": "99"
                            }
                        ]
                    }
                }
            ]
        }
    }



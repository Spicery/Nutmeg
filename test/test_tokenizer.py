import re
import json

from tokenizer import Token, Tokenizer

def tokenize( text ):
    return [ *Tokenizer()( text ) ]

def help_test( *testcases ):
    for testcase in testcases:
        assert testcase[1] == tokenize( testcase[0] )

def test_generates_integer_tokens():
    help_test(
        ("42", [Token(type="INT", value="42")]),
        ("1805", [Token(type="INT", value="1805")]),
        ("9999999999999999999999", [Token(type="INT", value="9999999999999999999999")]),
    )

def test_token_separation():
    help_test(
        ( "x + 3", [Token(type="ID", value="x"), Token(type="PLUS", value="+"), Token(type="INT", value="3")] )
    )

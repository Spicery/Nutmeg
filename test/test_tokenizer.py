import re
import json

from tokens import BasicToken
from tokenizer import tokenizer

def tokenize( text ):
    return [ *tokenizer( text ) ]

def help_test( *testcases ):
    for testcase in testcases:
        assert testcase[1] == tokenize( testcase[0] )

def test_generates_integer_tokens():
    help_test(
        ("42", [BasicToken("42", "INT")]),
        ("1805", [BasicToken("1805", "INT")]),
        ("9999999999999999999999", [BasicToken("9999999999999999999999", "INT")]),
    )

def test_token_separation():
    help_test(
        ( "x + 3", [BasicToken("x", "ID"), BasicToken("+", "PLUS"), BasicToken("3", "INT")] )
    )

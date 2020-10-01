from tokenizer import tokenizer
from tokens import IntToken

def tokenize( text ):
    return [ *tokenizer( text ) ]

def tokenizeOne( text ):
    ts = tokenize( text )
    assert len( ts ) == 1
    return ts[0]

def help_test( *testcases ):
    for testcase in testcases:
        assert testcase[1] == tokenize( testcase[0] )

def test_ints():
    for text in [ '42', '1805', '9999999999999999999999' ]:
        t = tokenizeOne( text )
        assert isinstance( t, IntToken )
        assert t.value() == text

def test_token_separation():
    ts = tokenize( "x + 3" )
    assert len( ts ) == 3

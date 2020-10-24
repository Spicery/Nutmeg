from tokenizer import tokenizer, IntToken, SyntaxToken, PunctuationToken

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

def __isPunctuation( token, category ):
    return not token.isPrefixer() and not token.isPostfixer() and isinstance( token, PunctuationToken ) and token.category() == category

def test_if_syntax():
    ts = tokenize( "if then elseif else endif" )
    assert 5 == len( ts )
    assert ts[0].isPrefixer() and isinstance( ts[0], SyntaxToken ) and ts[0].category() == "IF"
    assert __isPunctuation( ts[1], "THEN" )
    assert __isPunctuation( ts[2], "ELSE_IF" )
    assert __isPunctuation( ts[3], "ELSE" )
    assert __isPunctuation( ts[4], "END_IF" )

def test_eol_comment():
    ts = tokenize(
        """
        can you ### see this?
        """
    )
    assert 2 == len( ts )
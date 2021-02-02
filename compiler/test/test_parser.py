from parser import parseFromString
import json
import codetree

def parse( text ):
    return [ *parseFromString( text ) ]

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

def test_if2_ifPrefixMiniParser():
    if2 = parseOne( "if x then y endif" )
    assert isinstance( if2, codetree.IfCodelet )
    assert isinstance( if2.testPart(), codetree.IdCodelet )
    assert isinstance( if2.thenPart(), codetree.IdCodelet )
    assert isinstance( if2.elsePart(), codetree.SeqCodelet )
    assert len(if2.elsePart().body()) == 0

def test_if3_ifPrefixMiniParser():
    if3 = parseOne( "if x then y else z endif" )
    assert isinstance( if3, codetree.IfCodelet )
    assert isinstance( if3.testPart(), codetree.IdCodelet )
    assert isinstance( if3.thenPart(), codetree.IdCodelet )
    assert isinstance( if3.elsePart(), codetree.IdCodelet )

def test_if4_ifPrefixMiniParser():
    if4 = parseOne( "if x then y elseif p then q endif" )
    assert isinstance( if4, codetree.IfCodelet )
    assert isinstance( if4.testPart(), codetree.IdCodelet )
    assert isinstance( if4.thenPart(), codetree.IdCodelet )
    assert isinstance( if4.elsePart(), codetree.IfCodelet )
    if2 = if4.elsePart()
    assert isinstance( if2.testPart(), codetree.IdCodelet )
    assert isinstance( if2.thenPart(), codetree.IdCodelet )
    assert isinstance( if2.elsePart(), codetree.SeqCodelet )
    assert len(if2.elsePart().body()) == 0

def test_if5_ifPrefixMiniParser():
    if5 = parseOne( "if x then y elseif p then q else z endif" )
    assert isinstance( if5, codetree.IfCodelet )
    assert isinstance( if5.testPart(), codetree.IdCodelet )
    assert isinstance( if5.thenPart(), codetree.IdCodelet )
    assert isinstance( if5.elsePart(), codetree.IfCodelet )
    if3 = if5.elsePart()
    assert isinstance( if3.testPart(), codetree.IdCodelet )
    assert isinstance( if3.thenPart(), codetree.IdCodelet )
    assert isinstance( if3.elsePart(), codetree.IdCodelet )

def test_discardPostfixMiniParser1():
    e = parseOne( "0;;" )
    assert isinstance( e, codetree.SyscallCodelet)
    assert e.name() == "eraseAll"
    assert isinstance( e.arguments(), codetree.IntCodelet )

def test_discardPostfixMiniParser2():
    ''' eraseAll should discard a sequence '''
    es = [*parseFromString( "0, 1;;")]
    e = parseOne( "0, 1;;")
    assert isinstance( e, codetree.SyscallCodelet)
    assert e.name() == "eraseAll"
    assert isinstance( e.arguments(), codetree.SeqCodelet )

def test_discardPostfixMiniParser3():
    ''' eraseAll should affect only one statement '''
    el = [*parseFromString( "0; 1;;")]
    assert len( el ) == 2
    assert isinstance( el[0], codetree.IntCodelet)
    assert isinstance( el[1], codetree.SyscallCodelet)
    assert el[1].name() == "eraseAll"

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

def test_assignments_issue48():
    parseOne( 'x <- 99' ).toJSON() == {
        "kind": "assign",
        "lhs": {
            "kind": "id",
            "value": "x",
            "reftype": "get"
        },
        "rhs": {
            "kind": "int",
            "value": "99"
        }
    }

def test_assignments_issue48():
    parseOne( 'x := 99' ).toJSON() == {
        "kind": "binding",
        "lhs": {
            "kind": "id",
            "value": "x",
            "reftype": "get"
        },
        "rhs": {
            "kind": "int",
            "value": "99"
        }
    }

def test_optional_semi():
    text = """
    def foo():
        bar()
        gort()
    enddef
    """
    # Do we need a semi-colon? Should NOT throw an exception.
    codelet = parseOne( text )

def test_optional_semi_required():
    text = """
    def foo():
        f
        ()
    enddef
    """
    # Do we need a semi-colon? Should NOT throw an exception.
    codelet = parseOne( text )
    assert isinstance( codelet, codetree.BindingCodelet )
    rhs = codelet.rhs()
    assert isinstance( rhs, codetree.LambdaCodelet )
    body = rhs.body()
    assert isinstance( body, codetree.SeqCodelet )

def test_optional_semi_infix():
    text = """
    def foo():
        a +
        b
    enddef
    """
    # Do we need a semi-colon? Should NOT throw an exception.
    codelet = parseOne( text )
    assert isinstance( codelet, codetree.BindingCodelet )
    rhs = codelet.rhs()
    assert isinstance( rhs, codetree.LambdaCodelet )
    body = rhs.body()
    assert isinstance( body, codetree.SyscallCodelet )


def test_explicit_semi():
    text = """
    def foo():
        bar();
        gort();
    enddef
    """
    # Is the semi-colon permitted? Should NOT throw an exception.
    codelet = parseOne( text )

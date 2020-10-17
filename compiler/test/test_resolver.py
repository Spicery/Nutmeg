import codetree
import resolver
from resolver import resolveCodeTree

def test_resolveID():
    # Arrange
    jcodelet = {
        "kind": "id",
        "name": "x",
        "reftype": "get"
    }
    codelet = codetree.codeTreeFromJSONObject( jcodelet )
    # Act
    resolveCodeTree( codelet )
    # Assert
    assert 'global' == codelet.scope()

def test_resolveLiteral():
    # Arrange
    jcodelet = {
        "kind": "string",
        "value": "hello world"
    }
    codelet = codetree.codeTreeFromJSONObject( jcodelet )
    # Act
    resolveCodeTree( codelet )
    # Assert
    assert jcodelet == codelet.toJSON()

def test_resolveBinding():
    # Arrange
    jcodelet = {
        "kind": "binding",
        "lhs": {
            "kind": "id",
            "name": "x",
            "reftype": "var"
        },
        "rhs": {
            "kind": "string",
            "value": "hello world"
        }
    }
    codelet = codetree.codeTreeFromJSONObject( jcodelet )
    # Act
    resolveCodeTree( codelet )
    # Assert
    assert "global" == codelet.lhs().scope()

def test_resolveFunction():
    # Arrange
    jcodelet = {
        "kind": "lambda",
        "parameters": {
            "kind": "id",
            "name": "x",
            "reftype": "val"
        },
        "body": {
            "kind": "id",
            "name": "x",
            "reftype": "get"
        }
    }
    codelet = codetree.codeTreeFromJSONObject( jcodelet )
    # Act
    resolveCodeTree( codelet )
    # Assert
    assert "local" == codelet.body().scope()
    assert codelet.parameters().label() == codelet.body().label()
    assert codelet.parameters().nonassignable() and codelet.body().nonassignable()
    assert not codelet.parameters().immutable() and not codelet.body().immutable()

def test_resolveFunction2Variables():
    # Arrange
    jcodelet = {
        "kind": "lambda",
        "parameters": {
            "kind":"seq",
            "body": [
                {
                    "kind": "id",
                    "name": "x",
                    "reftype": "val"
                },
                {
                    "kind": "id",
                    "name": "y",
                    "reftype": "var"
                }
            ]
        },
        "body": {
            "kind": "id",
            "name": "x",
            "reftype": "get"
        }
    }
    codelet = codetree.codeTreeFromJSONObject( jcodelet )
    # Act
    resolveCodeTree( codelet )
    # Assert
    assert "local" == codelet.body().scope()
    params = [*codelet.parameters().members()]
    assert params[0].label() == codelet.body().label()
    assert params[1].label() != codelet.body().label()
    assert params[0].nonassignable() and codelet.body().nonassignable()
    assert not params[0].immutable() and not codelet.body().immutable()
    assert not params[1].nonassignable()
    assert not params[1].immutable()
    assert params[0].scope() == "local"
    assert params[1].scope() == "local"
    assert params[0].reftype() == "new"
    assert params[1].reftype() == "new"

def test_resolveIf():
    # Arrange
    jcodelet = {
        "kind": "if",
        "test": {
            "kind": "id",
            "name": "x",
            "reftype": "get"
        },
        "then":  {
            "kind": "id",
            "name": "y",
            "reftype": "get"
        },
        "else": {
            "kind": "id",
            "name": "z",
            "reftype": "get"
        }
    }
    codelet = codetree.codeTreeFromJSONObject( jcodelet )
    # Act
    resolveCodeTree( codelet )
    # Assert
    assert "global" == codelet.testPart().scope()
    assert "global" == codelet.thenPart().scope()
    assert "global" == codelet.elsePart().scope()

from parser import parseFromString

def parseOne( text ):
    e = [ *parseFromString( text ) ]
    assert len( e ) == 1
    return e[0]

import pytest
@pytest.mark.skip(reason="no way of currently testing this")
def test_if3_visitIfCodelet():
    # Arrange
    tree = parseOne( "def f(): x := 0; if t then x := 1; x else x := 2; x endif enddef" )
    # Act
    resolveCodeTree( tree )
    # Re-arrange for Assert
    _function = tree.rhs()
    assert isinstance( _function, codetree.LambaCodelet )
    _seq = _function.body()
    assert isinstance( _seq, codetree.SeqCodelet )
    _binding0 = _seq[0]
    assert isinstance( _binding0, codetree.BindingCodelet )
    _x0 = _binding0.lhs()
    assert isinstance( _x0, codetree.IdCodelet )
    _if3 = _seq[1]
    assert isinstance( _if3, codetree.IfCodelet )
    _seqThen = _if3.thenPart()
    assert isinstance( _seqThen, codetree.SeqCodelet )
    _x1_decl = _seqThen[0].lhs()
    assert isinstance( _x1_decl, codetree.IdCodelet )
    _x1_ref = _seqThen[1]
    assert isinstance( _x1_ref, codetree.IdCodelet )
    _seqElse = _if3.elsePart()
    assert isinstance( _seqElse, codetree.SeqCodelet )
    _x2_decl = _seqElse[0].lhs()
    assert isinstance( _x2_decl, codetree.IdCodelet )
    _x2_ref = _seqElse[1]
    assert isinstance( _x2_ref, codetree.IdCodelet )
    # Assert
    assert _x1_decl.label() == _x1_ref.label()
    assert _x2_decl.label() == _x2_ref.label()
    assert _x1_decl.label() != _x2_decl.label()
    assert _x0.label() != _x1_decl.label()
    assert _x0.label() != _x2_decl.label()



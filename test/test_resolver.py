import unittest
import io
import json
import codetree
import resolver
import sys

def resolveCodeTree( tree ):
    resolver.Resolver().resolveCodeTree( tree )

def test_resolveID():
    # Arrange
    jcodelet = {
        "kind": "id",
        "name": "x",
        "reftype": "get"
    }
    codelet = codetree.CodeTreeFromJSONObject( jcodelet )
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
    codelet = codetree.CodeTreeFromJSONObject( jcodelet )
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
    codelet = codetree.CodeTreeFromJSONObject( jcodelet )
    # Act
    resolveCodeTree( codelet )
    # Assert
    assert "global" == codelet.lhs().scope()

def test_resolveFunction():
    # Arrange
    jcodelet = {
        "kind": "function",
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
    codelet = codetree.CodeTreeFromJSONObject( jcodelet )
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
        "kind": "function",
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
    codelet = codetree.CodeTreeFromJSONObject( jcodelet )
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
    codelet = codetree.CodeTreeFromJSONObject( jcodelet )
    # Act
    resolveCodeTree( codelet )
    # Assert
    assert "global" == codelet.testPart().scope()
    assert "global" == codelet.thenPart().scope()
    assert "global" == codelet.elsePart().scope()

import unittest
import io
import json
import codetree
import resolver
import sys

def test_resolveID():
    # Arrange
    codelet = {
        "kind": "id",
        "name": "x",
        "reftype": "get"
    }
    codelet = codetree.deserialise( io.StringIO( json.dumps( codelet ) ) )
    # Act
    resolver.Resolver().resolveCodeTree( codelet )
    # Assert
    assert 'global' == codelet.scope()
    codelet.serialise(sys.stderr)

def test_resolveLiteral():
    # Arrange
    codelet = {
        "kind": "string",
        "value": "hello world"
    }
    codelet = codetree.deserialise( io.StringIO( json.dumps( codelet ) ) )
    # Act
    resolver.Resolver().resolveCodeTree( codelet )
    # Assert
    lkekjevjhbdevbhjdev

def test_resolveBinding():
    # Arrange
    codelet = {
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
    codelet = codetree.deserialise( io.StringIO( json.dumps( codelet ) ) )
    # Act
    resolver.Resolver().resolveCodeTree( codelet )
    # Assert
    codelet.serialise(sys.stderr)


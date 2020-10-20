import codetree
import optimizer

def test_if3_true_simplification():
    # Arrange
    jcodelet = {
        "kind": "if",
        "test": {
            "kind": "bool",
            "value": "true"
        },
        "then": {
            "kind": "id",
            "name": "x",
            "reftype": "get"
        },
        "else": {
            "kind": "id",
            "name": "y",
            "reftype": "get"
        }
    }
    tree = codetree.codeTreeFromJSONObject( jcodelet )
    # Act
    after = optimizer.Simplify()( tree )
    # Assert
    assert isinstance( after, codetree.IdCodelet )
    assert after.name() == 'x'

def test_if3_false_simplification():
    # Arrange
    jcodelet = {
        "kind": "if",
        "test": {
            "kind": "bool",
            "value": "false"
        },
        "then": {
            "kind": "id",
            "name": "x",
            "reftype": "get"
        },
        "else": {
            "kind": "id",
            "name": "y",
            "reftype": "get"
        }
    }
    tree = codetree.codeTreeFromJSONObject( jcodelet )
    # Act
    after = optimizer.Simplify()( tree )
    # Assert
    assert isinstance( after, codetree.IdCodelet )
    assert after.name() == 'y'

def test_if3_no_simplification():
    # Arrange
    jcodelet = {
        "kind": "if",
        "test": {
            "kind": "id",
            "name": "qqq",
            "reftype": "get"
        },
        "then": {
            "kind": "id",
            "name": "x",
            "reftype": "get"
        },
        "else": {
            "kind": "id",
            "name": "y",
            "reftype": "get"
        }
    }
    tree = codetree.codeTreeFromJSONObject( jcodelet )
    # Act
    after = optimizer.Simplify()( tree )
    # Assert
    assert after.toJSON() == jcodelet


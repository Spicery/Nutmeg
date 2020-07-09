import nutparse


def test_int_literals():
    assert nutparse.parse(42) == '{ "type": "int", "value" : "42", "radix": 10 }'


def test_string_literals():
    assert (
        nutparse.parse("meaning of life")
        == '{ "type": "str", "value" : "meaning of life" }'
    )

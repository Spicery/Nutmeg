import parser
import re
from collections import namedtuple

Token = namedtuple("Token", ["type", "value"])


def test_compiles_regex():
    assert type(parser.compile_regex({"BIND": "(?P<BIND>:=)",})) == re.Pattern


def test_generates_tokens():
    generator = parser.generate_tokens(re.compile(r"(?P<NUM>\d+)"), "42")
    expected_values = [Token(type="NUM", value="42")]

    range_index = 0
    for actual in generator:
        assert range_index + 1 <= len(
            expected_values
        ), "Too many values returned from range"
        assert expected_values[range_index] == actual
        range_index += 1

    assert range_index == len(expected_values), "Too few values returned from range"


def test_int_literals():
    assert parser.tokenize("42") == [Token(type="NUM", value="42")]


def test_string_literals():
    assert parser.tokenize("'string with single quotes'") == [
        Token(type="STRING", value="'string with single quotes'")
    ]
    assert parser.tokenize('"string with double quotes"') == [
        Token(type="STRING", value='"string with double quotes"')
    ]
    assert parser.tokenize("\"string within a 'string'\"") == [
        Token(type="STRING", value="\"string within a 'string'\"")
    ]

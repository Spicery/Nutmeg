import parser
import re
import json
from collections import namedtuple

Token = namedtuple("Token", ["type", "value"])


def help_test_token_generator(regex, input, expected_output):
    generator = parser.generate_tokens(re.compile(regex), input)
    range_index = 0

    for actual in generator:
        assert range_index + 1 <= len(
            expected_output
        ), "Too many values returned from range"
        assert expected_output[range_index] == actual
        range_index += 1

    assert range_index == len(expected_output), "Too few values returned from range"


def test_compiles_regex():
    assert type(parser.compile_regex({"BIND": "(?P<BIND>:=)",})) == re.Pattern


def test_generates_number_tokens():
    help_test_token_generator(
        r"(?P<NUM>\d+)", input="42", expected_output=[Token(type="NUM", value="42")],
    )

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


def test_generates_identifier_tokens():
    help_test_token_generator(
        r"(?i)(?P<ID>^([a-z])\w*)",
        input="x",
        expected_output=[Token(type="ID", value="x")],
    )


def test_parses_identifier():
    assert parser.Parser().parse("x") == json.dumps({"kind": "id", "name": "x"})


def test_parses_discard():
    assert parser.Parser().parse("_x") == json.dumps({"kind": "discard", "name": "_x"})


def test_parses_unsigned_int():
    assert parser.Parser().parse("99") == json.dumps({"kind": "int", "value": "99"})


def test_parses_signed_pos_int():
    assert parser.Parser().parse("+99") == json.dumps({"kind": "int", "value": "99"})


def test_parses_signed_neg_int():
    assert parser.Parser().parse("-99") == json.dumps({"kind": "int", "value": "-99"})

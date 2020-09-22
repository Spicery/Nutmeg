import re
import json

from tokenizer import Token, Tokenizer


def help_test_token_generator(input, expected_output):
    generator = Tokenizer().generate_tokens(text=input)
    range_index = 0

    for actual in generator:
        assert range_index + 1 <= len(
            expected_output
        ), "Too many values returned from range"
        assert expected_output[range_index] == actual
        range_index += 1

    assert range_index == len(expected_output), "Too few values returned from range"


def test_generates_integer_tokens():
    test_cases = [
        ("42", [Token(type="INT", value="42")]),
        ("1805", [Token(type="INT", value="1805")]),
        ("9999999999999999999999", [Token(type="INT", value="9999999999999999999999")]),
    ]

    for input, expected_output in test_cases:
        help_test_token_generator(
            input=input, expected_output=expected_output,
        )

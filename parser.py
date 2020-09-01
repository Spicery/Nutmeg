"""
parser -- parser module for the Nutmeg compiler
"""

import json
import os
import re
import sys
from collections import namedtuple

Token = namedtuple("Token", ["type", "value"])


def tokenize(text):
    token_spec = flatten(load_token_specification("token_specification.json"))
    regex_pattern = compile_regex(token_spec)

    tokens = []

    for tok in generate_tokens(regex_pattern, text):
        tokens.append(tok)

    return tokens


def load_token_specification(filename):
    """
    Load token specification in valid JSON from filename.
    Return a two-dimensional dictionary in form "<category>": {"<token name>" : "<regex pattern>"}.
    """
    with open(os.path.join(sys.path[0], filename), "r") as json_file:
        token_specification = json.load(json_file)

    return token_specification


def flatten(token_specification):
    """
    Take a two-dimensional dictionary in form "<category>": {"<token name>" : "<regex pattern>"}.
    Return a one-dimensional dictionary in form {"<token name>": "<regex pattern>"}.
    """
    flat_token_specification = {}

    for key, val in token_specification.items():
        if isinstance(val, dict):
            val = [val]
        if isinstance(val, list):
            for subdict in val:
                deeper = flatten(subdict).items()
                flat_token_specification.update({key2: val2 for key2, val2 in deeper})
        else:
            flat_token_specification[key] = val

    return flat_token_specification


def compile_regex(flat_token_specification):
    return re.compile(
        "|".join([regex_pattern for regex_pattern in flat_token_specification.values()])
    )


def generate_tokens(regex_pattern, text):
    scanner = regex_pattern.scanner(text)
    for m in iter(scanner.match, None):
        yield Token(m.lastgroup, m.group())

import re
from collections import namedtuple

import token_specification

Token = namedtuple("Token", ["type", "value"])


class Tokenizer:
    token_specification = token_specification

    def flatten(self, token_dictionary):
        """
        Take a two-dimensional dictionary in form "<category>": {"<token name>" : "<regex pattern>"}.
        Return a one-dimensional dictionary in form {"<token name>": "<regex pattern>"}.
        """
        flat_token_dictionary = {}

        for key, val in token_dictionary.items():
            if isinstance(val, dict):
                val = [val]
            if isinstance(val, list):
                for subdict in val:
                    deeper = flatten(subdict).items()
                    flat_token_dictionary.update({key2: val2 for key2, val2 in deeper})
            else:
                flat_token_dictionary[key] = val

        return flat_token_dictionary

    def compile_regex(flat_token_specification):
        return re.compile(
            "|".join(
                [regex_pattern for regex_pattern in flat_token_specification.values()]
            )
        )

    def generate_tokens(regex_pattern, text):
        scanner = regex_pattern.scanner(text)
        for m in iter(scanner.match, None):
            tok = Token(m.lastgroup, m.group())
            if tok.type != "WS":
                yield tok

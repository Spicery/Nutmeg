import re
from collections import namedtuple

from token_specification import token_spec

Token = namedtuple("Token", ["type", "value"])


class Tokenizer:
    def __init__(self):
        self.token_specification = self.__flatten(self.__import_token_spec())
        self.regex_pattern = self.__compile_regex()

    def __import_token_spec(self):
        return token_spec

    def __flatten(self, token_specification):
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
                    deeper = self.__flatten(subdict).items()
                    flat_token_specification.update(
                        {key2: val2 for key2, val2 in deeper}
                    )
            else:
                flat_token_specification[key] = val

        return flat_token_specification

    def __compile_regex(self):
        return re.compile(
            "|".join(
                [regex_pattern for regex_pattern in self.token_specification.values()]
            )
        )

    def generate_tokens(self, text=""):
        scanner = self.regex_pattern.scanner(text)
        for m in iter(scanner.match, None):
            tok = Token(m.lastgroup, m.group())
            if tok.type != "WS":
                yield tok

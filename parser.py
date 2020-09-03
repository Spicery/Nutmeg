"""
parser -- parser module for the Nutmeg compiler
"""

import json
import os
import re
import sys
from collections import namedtuple

import token_specification

Token = namedtuple("Token", ["type", "value"])


def flatten(token_dictionary):
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
        "|".join([regex_pattern for regex_pattern in flat_token_specification.values()])
    )


def generate_tokens(regex_pattern, text):
    scanner = regex_pattern.scanner(text)
    for m in iter(scanner.match, None):
        tok = Token(m.lastgroup, m.group())
        if tok.type != "WS":
            yield tok


class Parser:
    """
    docstring
    """

    def __init__(self):
        self.regex = compile_regex(flatten(token_specification.tokens))

    def parse(self, text):
        self.tokens = generate_tokens(self.regex, text)
        self.tok = None  # Last symbol consumed
        self.nexttok = None  # Next symbol tokenized
        self._advance()  # Load first lookahead token

        return self.identifier()

    def _advance(self):
        "Advance one token ahead"
        self.tok, self.nexttok = self.nexttok, next(self.tokens, None)

    def _accept(self, toktype):
        "Test and consume the next token if it matches toktype"
        if self.nexttok and self.nexttok.type == toktype:
            self._advance()
            return True
        else:
            return False

    def _expect(self, toktype):
        "Consume next token if it matches toktype or raise SyntaxError"
        if not self._accept(toktype):
            raise SyntaxError("Expected " + toktype)

    # Grammar rules

    def identifier(self):
        "TODO -- unsure of the EBNF here? "
        if self._accept("ID"):
            return {"kind": "id", "name": self.tok.value}

    def expr(self):
        "expression ::= term { ('+'|'-') term }"
        exprval = self.term()
        while self._accept("PLUS") or self._accept("MINUS"):
            op = self.tok.type
            right = self.term()
            if op == "PLUS":
                exprval += right
            elif op == "MINUS":
                exprval -= right
        return exprval

    def term(self):
        "term ::= factor { ('*'|'/') factor}"
        termval = self.factor()
        while self._accept("TIMES") or self._accept("DIVIDE"):
            op = self.tok.type
            right = self.factor()
            if op == "TIMES":
                termval *= right
            elif op == "DIVIDE":
                termval /= right
        return termval

    def factor(self):
        "factor ::= NUM | (expr)"

        if self._accept("NUM"):
            return int(self.tok.value)
        elif self._accept("LPAREN"):
            exprval = self.expr()
            self._expect("RPAREN")
            return exprval
        else:
            raise SyntaxError("Expected NUMBER or LPAREN")

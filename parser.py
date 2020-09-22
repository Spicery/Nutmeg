"""
parser -- parser module for the Nutmeg compiler
"""

import json
import os
import re
import sys

import codetree
from tokenizer import Token, Tokenizer


class Parser:
    """
    Recursive descent parser borrowed heavily from 
    https://www.oreilly.com/library/view/python-cookbook-3rd/9781449357337/.
    
    Each method implements a single grammar rule. Use the ._accept() method 
    to test and accept the current lookahead token. Use the ._expect() method 
    to exactly match and discard the next token on on the input (or raise a 
    SyntaxError if it doesn't match).
    """

    def __init__(self):
        self.regex = Tokenizer.compile_regex(
            Tokenizer.flatten(Tokenizer.token_specification)
        )

    def parse(self, text):
        self.tokens = Tokenizer.generate_tokens(self.regex, text)
        self.tok = None  # Last symbol consumed
        self.nexttok = None  # Next symbol tokenized
        self._advance()  # Load first lookahead token

        return self.expression()

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

    def expression(self):
        """
        Expression ::= 
        LiteralConstant |
        Identifier | TODO
        '(' Expression? ')' | TODO
        Expression '(' Expression ')' | TODO
        Expression InfixOperator Expression | TODO
        LetExpression | TODO
        IfExpression | TODO
        SwitchExpression | TODO
        LoopExpression | TODO
        LambdaExpression TODO
        """
        expr = self.literal_constant() or self.identifier()
        return expr

    def identifier(self):
        "Identifier ::= [https://www.w3.org/TR/xml-names/#NT-NCName]"
        if self._accept("id"):
            return codetree.IdCodelet(kind="id", name=self.tok.value, reftype="var")
        elif self._accept("discard"):
            return codetree.IdCodelet(
                kind="discard", name=self.tok.value, reftype="var"
            )

    def literal_constant(self):
        "LiteralConstant ::= String TODO | Number | Boolean TODO | Null TODO"
        if self._accept("int"):
            return codetree.IntCodelet(kind="int", value=self.tok.value)

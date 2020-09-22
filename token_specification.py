# token_spec is the specification for Nutmeg's tokens
#
# token_spec is used by Tokenizer to generate a stream of tokens in form
# e.g. Token(type="int", value="42")
#
# Each key is a category of the syntax: "literal_constants", "operators" etc.
# Categories are for ease of human reading only: they have no functional utility
# and can be changed.
#
# Each value is a dictionary of the token type and its matching regex
# pattern: { <*token type*>: r"*matching regex pattern*" }.
#
# The named capturing group within each regex pattern (e.g. "?P<int>")
# is required for Tokenizer to match regex pattern with token type.


token_spec = {
    "identifier": {"ID": r"(?i)(?P<ID>^([a-z])\w*)"},
    "literal_constants": {
        "INT": r"(?P<INT>^(\+|-)?\d+$)",
        "STRING": r"(?P<STRING>\".*\"|'.*')",
        "WHITESPACE": r"(?P<WS>\s+)",
    },
    "operators": {
        "BIND": r"(?P<BIND>:=)",
        "ASSIGNMENT": r"(?P<ASSIGN><-)",
        "UPDATE_ELEMENT": r"(?P<UPDATE_ELEMENT><--)",
        "COPY_AND_SET": r"(?P<COPY_AND_SET><==)",
        "PLUS": r"(?P<PLUS>\+)",
        "MINUS": r"(?P<MINUS>-)",
        "TIMES": r"(?P<TIMES>\*)",
        "DIVIDE": r"(?P<DIVIDE>/)",
    },
    "separators": {
        "TERMINATE_STATEMENT": r"(?P<TERMINATE_STATEMENT>;)",
        "LPAREN": r"(?P<LPAREN>\()",
        "RPAREN": r"(?P<RPAREN>\))",
    },
    "keywords": {
        "DEC_VARIABLE": r"(?P<DEC_VARIABLE>var)",
        "DEC_IMMUTABLE": r"(?P<DEC_IMMUTABLE>val)",
        "DEC_CONSTANT": r"(?P<DEC_CONSTANT>const)",
        "DEC_FUNCTION_1": r"(?P<DEC_FUNCTION_1>def)",
        "DEC_FUNCTION_2": r"(?P<DEC_FUNCTION_2>function)",
    },
    "comments": {
        "COMMENT_LINE": r"(?P<COMMENT_LINE>###)",
        "COMMENT_BLOCK_START": r"(?P<COMMENT_BLOCK_START>##/()",
        "COMMENT_BLOCK_FINISH": r"(?P<COMMENT_BLOCK_FINISH>##))",
    },
}

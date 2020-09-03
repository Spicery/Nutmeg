tokens = {
    "literals": {
        "NUM": r"(?P<NUM>\d+)",
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

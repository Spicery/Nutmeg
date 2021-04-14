SYSCONSTS = {
    # Not in a module
    "println",
    "showMe",
    # Range module
    "range",
    "..<",
    "...",
    "[x..<y]",
    "[x...y]",
    # Arith module
    "+",
    "sum", "product",
    "min", "max",
    "-", "*",
    "quot", "rem",
    # Bitwise module
    "AND", "OR", "XOR", "NOT", "LSHIFT", "RSHIFT",
    # Not in a module
    "<=", "<", ">=", ">",
    "==", "!=", "not",
    "countArguments",
    "dup",
    # String functions - which will need to be promoted to methods soon enough.
    "get", "length",
    "startsWith", "endsWith",
    "contains",
    "trim", "indexOf",
    "++", "newString",
    "split", "join",
    "substring",
    # Character functions
    "isLowercase", "isUppercase",
    "lowercase", "uppercase",
    # Ref functions
    "newVarRef", "newValRef", "sealObject", "isRef",
}

def isSysconst( name ):
    return name in SYSCONSTS
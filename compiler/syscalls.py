SYSCONSTS = (
    set((
        # Not in a module
        "println",
        "showMe",
        # Range module
        "..<",
        "...",
        "[x..<y]",
        "[x...y]",
        # Arith module
        "+",
        "sum",
        "-",
        "*",
        # Not in a module
        "<=", "<", ">=", ">",
        "==", "!=", "not",
        # String functions - which will need to be promoted to methods soon enough.
        "get", "length",
        "startsWith", "endsWith",
        "contains",
        "trim", "indexOf",
        "++", "newString",
    ))
)

def isSysconst( name ):
    return name in SYSCONSTS
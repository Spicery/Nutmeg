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
        "sum", "product",
        "min", "max",
        "-",
        "*",
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
    ))
)

def isSysconst( name ):
    return name in SYSCONSTS
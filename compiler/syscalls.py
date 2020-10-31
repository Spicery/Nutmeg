SYSCONSTS = (
    set((
        "println",
        "showMe",
        "..<",
        "...",
        "[x..<y]",
        "[x...y]",
        "+",
        "sum",
        "-",
        "*",
        "<=", "<", ">=", ">",
        "assert", "==", "!=",
    ))
)

def isSysconst( name ):
    return name in SYSCONSTS
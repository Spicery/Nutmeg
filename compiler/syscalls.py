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
    ))
)

def isSysconst( name ):
    return name in SYSCONSTS
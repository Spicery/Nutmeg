compile_mode :pop11 +strict;

uses $-nutmeg$-arity_expr;

section $-nutmeg;

define newSingleValue( expr );
    lvars a = expr.arity_expr;
    if a.countArity == 1 and a.isExactArity then
        expr
    else
        newFixedArity( newExactArity( 1 ), expr )
    endif
enddefine;

endsection;

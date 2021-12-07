compile_mode :pop11 +strict;

section $-nutmeg;

define arity_expr( expr );
    if expr.isConstant then
        newExactArity( 1 )
    elseif expr.isId then
        newExactArity( 1 )
    elseif expr.isSeq then
        appdata( 0, expr, procedure(); arity_expr().sum_arities endprocedure )
    elseif expr.isApply then
        newInexactArity( 0 )
    else
        newInexactArity( 0 )
    endif
enddefine;

endsection;

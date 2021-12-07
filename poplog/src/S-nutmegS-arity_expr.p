compile_mode :pop11 +strict;

uses $-nutmeg$-nutmeg_arity;


section $-nutmeg;

constant zero_arity = newExactArity( 0 );

define arity_expr( expr );
    if expr.isConstant then
        newExactArity( 1 )
    elseif expr.isId then
        newExactArity( 1 )
    elseif expr.isSeq then
        appdata( zero_arity, expr, procedure(); arity_expr().sum_arities endprocedure )
    elseif expr.isApply then
        newInexactArity( 0 )
    else
        newInexactArity( 0 )
    endif
enddefine;

endsection;

compile_mode :pop11 +strict;

section $-nutmeg;

defclass Arity {
    isExactArity,
    countArity
};

define sum_arities( a, b );
    consArity(
        isExactArity( a ) and isExactArity( b ),
        countArity( a ) + countArity( b )
    )
enddefine;

define arity_expr( expr );
    if expr.isConstant then
        consArity( true, 1 )
    elseif expr.isId then
        consArity( true, 1 )
    elseif expr.isSeq then
        appdata( 0, expr, procedure(); arity_expr().sum_arities endprocedure )
    elseif expr.isApply then
        consArity( false, 0 )
    else
        consArity( false, 0 )
    endif
enddefine;

endsection;

compile_mode :pop11 +strict;

section $-nutmeg;

defclass Arity {
    isExactArity,
    countArity
};

define newArity(count);
    lvars exact = true;
    if count.isboolean then
        count -> exact;
        () -> count
    endif;
    consArity( exact, count )
enddefine;

define newExactArity( n );
    consArity( true, n )
enddefine;

define newInexactArity( n );
    consArity( false, n )
enddefine;

define sum_arities( a, b );
    consArity(
        isExactArity( a ) and isExactArity( b ),
        countArity( a ) + countArity( b )
    )
enddefine;

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

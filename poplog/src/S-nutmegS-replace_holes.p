compile_mode :pop11 +strict;

section $-nutmeg;

vars holes_found = [];
vars holes_count;

define do_replace( expr );
    if expr.isConstant or expr.isId then
        expr
    elseif expr.isHole then
        holes_count + 1 -> holes_count;
        lvars t = consword( sprintf( holes_count, '<hole:%p>' ) );
        conspair( t, holes_found ) -> holes_found;
        newId( t )
    elseif expr.isSeq or expr.isApply or expr.isFixedArity then
        mapdata( expr, do_replace )
    elseif expr.isFn then
        newFn( expr.nameFn, expr.paramsFn, expr.bodyFn.do_replace )
    elseif expr.isBind then
        newBind( expr.patternBind, expr.valueBind.do_replace )
    else
        mishap( 'Internal error: cannot do hole substitution on this', [ ^expr ] )
    endif
enddefine;

define replace_holes( expr );
    dlocal holes_found;
    dlocal holes_count = 0;
    do_replace( expr ), holes_found
enddefine;

endsection;
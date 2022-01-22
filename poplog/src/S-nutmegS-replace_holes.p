compile_mode :pop11 +strict;

section $-nutmeg;

vars holes_found = [];
vars holes_count;

define do_replace( expr );
    if expr.isConstant or expr.isId or expr == false then
        expr
    elseif expr.isHole then
        holes_count + 1 -> holes_count;
        lvars t = consword( sprintf( holes_count, '<hole:%p>' ) );
        conspair( t, holes_found ) -> holes_found;
        newId( t )
    elseif expr.isFixedArity then
        newFixedArity( expr.arityFixedArity, expr.valueFixedArity.do_replace )
    elseif expr.isSeq or expr.isApply or expr.isIf or expr.isSwitch or expr.isWhenThen or expr.isCaseThen then
        mapdata( expr, do_replace )
    elseif expr.isFn then
        ;;; Is this correct? TODO
        newFn( expr.nameFn, expr.paramsFn, expr.bodyFn.do_replace )
    elseif expr.isBind then
        newBind( expr.patternBind, expr.valueBind.do_replace )
    elseif expr.islist then
        maplist( expr, do_replace )
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
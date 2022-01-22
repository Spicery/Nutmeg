compile_mode :pop11 +strict;

section $-nutmeg;
vars nutmeg_tree = _;

defclass Constant {
    valueConstant
};
vars procedure newConstant = consConstant;

defclass SharedData {
    isLocalSharedData,
    isOuterSharedData,
    isAssignableSharedData,
    isDiscardSharedData
};
define newSharedData();
    consSharedData( true, false, false, false )
enddefine;

defclass Id {
    nameId,
    sharedDataId,
    idRefId
};
define newId( name );
    consId( name, newSharedData(), _ )
enddefine;

vars procedure isLocalId = sharedDataId <> isLocalSharedData;
vars procedure isAssignableId = sharedDataId <> isAssignableSharedData;
vars procedure hasNonLocalRefToId = sharedDataId <> isOuterSharedData;
vars procedure isDiscardId = sharedDataId <> isDiscardSharedData;

define shareData( parent, child );
    parent.sharedDataId -> child.sharedDataId
enddefine;

procedure( id ) with_props printId;
    if id.isLocalId then
        cucharout( '<' );
        if id.hasNonLocalRefToId then
            cucharout( 'Non' )
        endif; 
        cucharout( 'Local ' );
        appdata( id.nameId, cucharout );
        cucharout( '>' )
    else
        cucharout( '<Global ' );
        appdata( id.nameId, cucharout );
        cucharout( '>' )        
    endif 
endprocedure -> class_print( Id_key );

defclass Seq;
lconstant empty_seq = consSeq(0);
define newSeq( n );

    define lconstant has_seq_arg( n );
        lvars i;
        for i from 1 to n do
            returnif( subscr_stack( i ).isSeq )( true )
        endfor;
        return( false )
    enddefine;

    if n == 0 then
        empty_seq
    elseif n == 1 then
        ;;; Simply return
    elseif has_seq_arg( n ) then
        ;;; Slow construction.
        lvars list = [];
        while n > 0 do
            lvars item = ();    ;;; pop from stack
            n - 1 -> n;         ;;; decrement count
            if item.isSeq then
                item.destSeq + n -> n
            else
                conspair( item, list ) -> list
            endif
        endwhile;
        consSeq(#|
            until null( list ) do
                sys_grbg_destpair( list ) -> list
            enduntil
        |#)
    else
        consSeq( n )
    endif
enddefine;

defclass Apply {
    fnApply,
    argsApply
};
vars procedure newApply = consApply;

defclass Fn {
    nameFn,
    paramsFn,
    bodyFn
};
vars procedure newFn = consFn;

defclass Bind {
    patternBind,
    valueBind
};
vars procedure newBind = consBind;

defclass Assign {
    targetAssign,
    sourceAssign
};
vars procedure newAssign = consAssign;

defclass Hole {
};
vars procedure newHole = consHole;

defclass FixedArity {
    arityFixedArity,
    valueFixedArity
};
vars procedure newFixedArity = consFixedArity;

defclass CaseThen {
    predicateCaseThen,
    actionCaseThen
};
vars procedure newCaseThen = consCaseThen;

defclass Switch {
    selectorSwitch,
    caseThenListSwitch,
    elseSwitch
};
vars procedure newSwitch = consSwitch;

defclass FallThru;
vars FallThru = consFallThru( 0 );

defclass WhenThen {
    whenWhenThen,
    thenWhenThen
};
vars procedure newWhenThen = consWhenThen;

defclass If {
    whenThenListIf,
    elseIf
};
vars procedure newIf = consIf;

defclass In {
    idIn,
    valueIn
};
vars procedure newIn = consIn;

defclass For {
    queryFor,
    bodyFor
};
vars procedure newFor = consFor;

endsection;

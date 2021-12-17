compile_mode :pop11 +strict;

section $-nutmeg => nutmeg_valof;

;;; --- Globals ----------------------------------------------------------------

defclass GlobalScope {
    packagesGlobalScope
};

defclass IdRef {
    contIdRef
};

vars procedure nutmeg_packages =
    newanyproperty(
        [], 8, 1, false,
        false, false, false,
        false, false
    );
vars nutmeg_global_scope = consGlobalScope( nutmeg_packages );

define fetchIdRefByNameInGlobalScope( name, gscope, allocate ) -> result;
    lvars packages = gscope.packagesGlobalScope;
    lvars idref = packages( name );
    if idref then
        idref
    elseif allocate then
        consIdRef( _ ) ->> packages( name );
    else
        mishap( 'Unknown identifier', [ ^name ] )
    endif -> result;
enddefine;

define declareInGlobalScope( id, gscope );
    fetchIdRefByNameInGlobalScope( id.nameId, gscope, true ) -> id.idRefId;
enddefine;

define nutmeg_valof( w );
    contIdRef( fetchIdRefByNameInGlobalScope( w, nutmeg_global_scope, false ) )
enddefine;

define updaterof nutmeg_valof( v, w );
    v -> contIdRef( fetchIdRefByNameInGlobalScope( w, nutmeg_global_scope, true ) )
enddefine;

;;; --- Locals- ----------------------------------------------------------------

defclass LocalScope {
    idListLocalScope,
    isDynamicLocalScope,        ;;; Static or Dynamic
    previousScopeLocalScope
};
define newLocalScope( old_scope );
    consLocalScope( [], false, old_scope )
enddefine;

define findInLocalScope( id, lscope );
    lvars name = id.nameId;
    lvars i;
    for i in lscope.idListLocalScope do
        if i.nameId == name then
            return( i )
        endif
    endfor;
    return( false )
enddefine;

;;; --- Scopes -----------------------------------------------------------------

define declareInScope( id, scope );
    if scope.isLocalScope then
        true -> id.isLocalId;
        conspair( id, scope.idListLocalScope ) -> scope.idListLocalScope;
    elseif scope.isGlobalScope then
        false -> id.isLocalId;
        if id.isAssignableId then
            mishap( 'Global variables cannot be declared to be assignable', [% id.nameId %] )
        endif;
        declareInGlobalScope( id, scope );
    else 
        mishap( 'Internal error invalid scope', [ ^scope ] )
    endif
enddefine;

define checkUndeclaredInScope( id, scope );
    if scope.isLocalScope then
        lvars xid = findInLocalScope( id, scope );
        if xid then
            mishap( 'Already declared', [% id.nameId %] )
        endif
    endif
enddefine;

define resolveIdInScope( id, scope );

    define lconstant resolve_and_mark_id_in_scope( id, scope, is_non_local );
        if scope.isGlobalScope then
            ;;; It is a global variable.
            false -> id.isLocalId;
            false -> id.isAssignableId;
            declareInGlobalScope( id, scope )
        elseif scope.isLocalScope then
            lvars declared_id = findInLocalScope( id, scope );
            if declared_id then
                ;;; It is a local variable.
                true -> id.isLocalId;
                declared_id.isAssignableId -> id.isAssignableId;
                shareLocalDataId( declared_id, id );
                if is_non_local then
                    true -> declared_id.hasNonLocalRefToId;
                endif
            else
                resolve_and_mark_id_in_scope( id, scope.previousScopeLocalScope, is_non_local or scope.isDynamicLocalScope );
            endif
        else
            mishap( 'Internal error', [] )
        endif
    enddefine;

    resolve_and_mark_id_in_scope( id, scope, false )
enddefine;

;;; --- resolve ----------------------------------------------------------------

define resolve_pattern( patt, scope );
    if patt.isId then
        checkUndeclaredInScope( patt, scope );
        declareInScope( patt, scope )
    elseif patt.isSeq then
        lvars p;
        for p in_vectorclass patt do
            resolve_pattern( p, scope )
        endfor;
    elseif patt.isConstant then
        ;;; nothing
    else
        mishap( 'Not implemented yet', [ ^p ] )
    endif
enddefine;

define resolve( tree, scope );
    if tree.isId then
        resolveIdInScope( tree, scope )
    elseif tree.isFn then
        lvars lscope = newLocalScope( scope );
        true -> lscope.isDynamicLocalScope;
        resolve_pattern( tree.paramsFn, lscope );
        resolve( tree.bodyFn, lscope );
    elseif tree.isBind then
        resolve( tree.valueBind, scope );
        resolve_pattern( tree.patternBind, scope )
    elseif tree.isAssign then
        resolve( tree.targetAssign, scope );
        resolve( tree.sourceAssign, scope );
    elseif tree.isSeq then
        lvars e;
        for e in_vectorclass tree do
            resolve( e, scope )
        endfor
    elseif tree.isApply then
        resolve( tree.fnApply, scope );
        resolve( tree.argsApply, scope );
    elseif tree.isFixedArity then
        resolve( tree.valueFixedArity, scope )
    elseif tree.isSwitch then
        resolve( tree.selectorSwitch, scope );
        lvars ct;
        for ct in tree.caseThenListSwitch do
            lvars lscope = newLocalScope( scope );
            resolve_pattern( ct.predicateCaseThen, lscope );
            resolve( ct.actionCaseThen, lscope );
        endfor;
        if tree.elseSwitch then 
            resolve( tree.elseSwitch, scope ) 
        endif;
    elseif tree.isConstant or tree.isHole then
        ;;; Do nothing
    else
        mishap( 'Internal error: unhandled case in resolver', [ ^tree ] )
    endif
enddefine;

;;; This procedure annotates the identifiers in a tree as to whether they
;;; are global or local and whether or not they are assignable.
define nutmeg_resolve( tree );
    resolve( tree, nutmeg_global_scope )
enddefine;

endsection;

compile_mode :pop11 +strict;

uses nutmeg_toplevel_print;
uses nutmeg_loops;
uses $-nutmeg$-nutmeg_tree;
uses $-nutmeg$-nutmeg_parse;
uses $-nutmeg$-nutmeg_resolve;
uses $-nutmeg$-nutmeg_builtins;
uses tokeniser;

section $-nutmeg => nutmeg_compiler, nutmeg_initialise_loop;

vars procedure plant_table = (
    newanyproperty( 
        [], 8, 1, false,
        false, false, false,
        false, false
    )
);

define plant_expr( expr );
    lvars p = plant_table( expr.datakey );
    if p then   
        p( expr )
    else
        mishap( 'Do not know how to compile this', [ ^expr ] )
    endif;
enddefine;

procedure( expr ) with_props plant_binding;
    plant_expr( expr.valueBind );
    lvars pattern = expr.patternBind;
    if pattern.isId then
        if pattern.isLocalId then
            sysLVARS( pattern.nameId, 0 );
            sysPOP( pattern.nameId );
        else
            lvars idref = pattern.idRefId;
            sysPUSHQ( idref );
            sysUFIELD( 1, IdRef_key.class_spec, false, false );
        endif
    else 
        mishap( 'Only simple identifiers supported at the moment', [ ^pattern ] )   
    endif
endprocedure -> plant_table( Bind_key );


procedure( expr ) with_props plant_assign;
    plant_expr( expr.sourceAssign );
    lvars target = expr.targetAssign;
    if target.isId then
        if target.isAssignableId then
            if target.isLocalId then
                sysPOP( target.nameId );
            else
                lvars idref = target.idRefId;
                sysPUSHQ( idref );
                sysUFIELD( 1, IdRef_key.class_spec, false, false );
            endif
        else
            mishap( 'Trying to assign to protected variable', [% target.nameId %] )
        endif
    else 
        mishap( 'Assignment is limited to simple identifiers at the moment', [ ^target ] )   
    endif
endprocedure -> plant_table( Assign_key );

procedure( expr ) with_props plant_constant;
    sysPUSHQ( expr.valueConstant )
endprocedure -> plant_table( Constant_key );

procedure( expr ) with_props plant_id;
    lvars name = expr.nameId;
    if expr.isLocalId then
        sysPUSH( name )
    else 
        lvars idref = expr.idRefId;
        sysPUSHQ( idref );
        sysFIELD( 1, IdRef_key.class_spec, false, false );
    endif
endprocedure -> plant_table( Id_key );

procedure( expr ) with_props plant_seq;
    appdata( expr, plant_expr )
endprocedure -> plant_table( Seq_key );

procedure( expr ) with_props plant_apply;
    dlocal pop_new_lvar_list;
    lvars args_arity = arity_expr( expr.argsApply );
    if args_arity.isExactArity then
        plant_expr( expr.argsApply );
        sysPUSHQ( args_arity.countArity );
    else
        lvars stack_count = sysNEW_LVAR();
        sysCALL( "stacklength" );
        sysPOP( stack_count );
        plant_expr( expr.argsApply );
        sysCALL( "stacklength" );
        sysPUSH( stack_count );
        sysCALL( "-" );
    endif;
    plant_expr( expr.fnApply );
    sysCALLS( _ );
endprocedure -> plant_table( Apply_key );

define dumpParams( args );
    if args.isId then
        args.nameId
    elseif args.isSeq then
        appdata( args, dumpParams )
    endif        
enddefine;

define consrevlist( N );
    if N <= 0 then
        nil
    else
        lvars h = ();
        lvars t = consrevlist( N - 1 );
        conspair( h, t )
    endif
enddefine;

define gatherParamsInReverse( args ) -> ( L, N );
    consrevlist(#| args.dumpParams |# ->> N) -> L
enddefine;

procedure( expr ) with_props plant_fn;
    lvars name = expr.nameFn;
    lvars ( params, N ) = expr.paramsFn.gatherParamsInReverse;
    lvars N = length( params );
    lvars body = expr.bodyFn;
    sysPROCEDURE( name, N );
    lvars a;
    for a in params do
        sysLVARS( a, 0 );
        sysPOP( a );
    endfor;
    plant_expr( body );
    sysPUSHQ( sysENDPROCEDURE() );
    sysPUSHQ( N );
    sysCALLQ( check_exact_arity );
endprocedure -> plant_table( Fn_key );

define switch_failure( item );
    mishap( 'No matching case in switch expression', [ ^item ] )
enddefine;

procedure( expr ) with_props plant_switch;
    dlocal pop_new_lvar_list;
    lvars tmp = sysNEW_LVAR();
    plant_expr( expr.selectorSwitch );
    sysPOP( tmp );
    lvars endswitch_label = sysNEW_LABEL();
    lvars pending_fallthru_labels = [];
    lvars ct;
    for ct in expr.caseThenListSwitch do
        plant_expr( ct.predicateCaseThen );
        sysPUSH( tmp );
        sysCALL( "=" );
        lvars a = ct.actionCaseThen;
        if a.isFallThru then
            lvars fallthru_lab = sysNEW_LABEL();
            conspair( fallthru_lab, pending_fallthru_labels ) -> pending_fallthru_labels;
            sysIFSO( fallthru_lab );
        else
            lvars next_case_label = sysNEW_LABEL();
            sysIFNOT( next_case_label );
            lvars lab;
            for lab in pending_fallthru_labels do
                sysLABEL( lab )
            endfor;
            [] -> pending_fallthru_labels;
            plant_expr( a );
            sysGOTO( endswitch_label );
            sysLABEL( next_case_label );
        endif;
    endfor;
    if expr.elseSwitch then
        plant_expr( expr.elseSwitch )
    else
        sysPUSHQ( tmp );
        sysCALLQ( switch_failure )
    endif;
    sysLABEL( endswitch_label );
endprocedure -> plant_table( Switch_key );

procedure( for_expr ) with_props plant_for;
    dlocal pop_new_lvar_list;
    lvars q = for_expr.queryFor;
    lvars b = for_expr.bodyFor;
    
    lvars advancer = sysNEW_LVAR();
    lvars that_which_changes = sysNEW_LVAR();
    lvars that_that_stays_the_same = sysNEW_LVAR();
    
    plant_expr( q.valueIn );
    sysCALL( "nutmeg_initialise_loop" );
    sysPOP( that_that_stays_the_same );
    sysPOP( that_which_changes );
    sysPOP( advancer );
    
    lvars loop_entry = sysNEW_LABEL();
    lvars loop_exit = sysNEW_LABEL();
    lvars loop_body_start = sysNEW_LABEL();
    sysGOTO( loop_entry );
    
    sysLABEL( loop_body_start );
    sysPOP( that_which_changes );

    sysLBLOCK( popexecute );
    
    ;;; Bind the pattern.
    sysLVARS( q.idIn.nameId, 0 );
    sysPOP( q.idIn.nameId );

    ;;; Run the body.
    plant_expr( for_expr.bodyFor );

    sysENDLBLOCK();

    sysLABEL( loop_entry );
    sysPUSH( that_which_changes );
    sysPUSH( that_that_stays_the_same );
    sysCALL( advancer );
    sysIFSO( loop_body_start );
    
    sysLABEL( loop_exit );
endprocedure -> plant_table( For_key );

define plant_expr_with_count( expr );
    lvars a = arity_expr( expr );
    if a.isExactArity then
        plant_expr( expr );
        sysPUSHQ( a.countArity );
    else
        lvars stklen = sysNEW_LVAR();
        sysCALL( "stacklength" );
        sysPOP( stklen );
        plant_expr( expr );
        sysCALL( "stacklength" );
        sysPUSH( stklen );
        sysCALL( "-" );
    endif
enddefine;

procedure( fixed_expr ) with_props plant_fixed_expr;

    define check_counts( actual, expected );
        returnif( actual == expected );
        if actual > expected then
            mishap( sprintf( 'Too many arguments (actual: %p, expected: %p)', [ ^actual ^expected ] ), [] )
        else
            mishap( sprintf( 'Insufficient arguments (actual: %p, expected: %p)', [ ^actual ^expected ] ), [] )
        endif
    enddefine;

    define check_enough( actual, expected );
        returnif( actual >= expected );
        mishap( 'Insufficient arguments', [] )
    enddefine;

    dlocal pop_new_lvar_list;
    lvars a = fixed_expr.arityFixedArity;
    lvars b = arity_expr( fixed_expr.valueFixedArity );
    if a == b then
        plant_expr( fixed_expr.valueFixedArity )
    elseif a.isExactArity then
        plant_expr_with_count( fixed_expr.valueFixedArity );
        sysPUSHQ( a.countArity );
        sysCALLQ( check_counts );
    elseif a.countArity == 0 then
        plant_expr( fixed_expr.valueFixedArity )
    elseif b.countArity >= a.countArity then
        plant_expr( fixed_expr.valueFixedArity )
    else
        plant_expr_with_count( fixed_expr.valueFixedArity );
        sysPUSHQ( a.countArity );
        sysCALLQ( check_enough );
    endif
endprocedure -> plant_table( FixedArity_key );

;;;
;;; Here we use -proglist_state- and -proglist_new_state-, even though it is
;;; overkill, to convert various kinds of source into a character repeater.
;;; We will actually override proglist so that it becomes a dynamic list of
;;; character codes.
;;;
define procedure nutmeg_compiler( source );
    ;;; dlocal proglist_state = proglist_new_state(source);
    dlocal cucharin = source.isstring and source.discin or source;
    dlocal proglist = cucharin.nutmeg_tokeniser.pdtolist;
    ;;; lvars itemiser = proglist.isdynamic;
    ;;; item_chartype( `,`, itemiser ) -> item_chartype( `\\`, itemiser );
    dlocal pop_pr_quotes = true;
    procedure();
        dlocal popnewline = true;
        until null(proglist) do
            lvars e = read_optexpr();
            if e then
                nutmeg_resolve( e );
                plant_expr( e );
                sysCALL( "nutmeg_toplevel_print" );
                sysEXECUTE();
            endif;
            pop11_need_nextreaditem([, ; ^newline]) -> _;
        enduntil;
    endprocedure.sysCOMPILE;
enddefine;

endsection;

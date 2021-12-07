compile_mode :pop11 +strict;

uses $-nutmeg$-nutmeg_tree;
uses $-nutmeg$-nutmeg_parse;
uses $-nutmeg$-nutmeg_resolve;

section $-nutmeg => nutmeg_compiler;

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
        lvars idref = declare_name( pattern.nameId );
        sysPUSHQ( idref );
        sysUFIELD( 1, IdRef_key.class_spec, false, false );
    else 
        mishap( 'Only simple identifiers supported at the moment', [ ^pattern ] )   
    endif
endprocedure -> plant_table( Bind_key );

procedure( expr ) with_props plant_constant;
    sysPUSHQ( expr.valueConstant )
endprocedure -> plant_table( Constant_key );

procedure( expr ) with_props plant_id;
    lvars idref = resolve( expr );
    sysPUSHQ( idref );
    sysFIELD( 1, IdRef_key.class_spec, false, false );
endprocedure -> plant_table( Id_key );

procedure( expr ) with_props plant_seq;
    appdata( expr, plant_expr )
endprocedure -> plant_table( Seq_key );

procedure( expr ) with_props plant_apply;
    dlocal pop_new_lvar_list;
    lvars stack_count = sysNEW_LVAR();
    sysCALL( "stacklength" );
    sysPOP( stack_count );
    plant_expr( expr.argsApply );
    sysCALL( "stacklength" );
    sysPUSH( stack_count );
    sysCALL( "-" );
    plant_expr( expr.fnApply );
    sysCALLS( _ );
endprocedure -> plant_table( Apply_key );

;;;
;;; Here we use -proglist_state- and -proglist_new_state-, even though it is
;;; overkill, to convert various kinds of source into a character repeater.
;;; We will actually override proglist so that it becomes a dynamic list of
;;; character codes.
;;;
define procedure nutmeg_compiler( source );
    dlocal proglist_state = proglist_new_state(source);
    dlocal pop_pr_quotes = true;
    procedure();
        dlocal popnewline = true;
        until null(proglist) do
            lvars e = read_optexpr();
            if e then
                plant_expr( e );
                sysPUSHQ( true );
                sysCALL( "sysprarrow" );
                sysEXECUTE();
            endif;
            pop11_need_nextreaditem([, ; ^newline]) -> _;
        enduntil;
    endprocedure.sysCOMPILE;
enddefine;

endsection;

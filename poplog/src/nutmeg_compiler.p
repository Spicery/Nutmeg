compile_mode :pop11 +strict;

section $-nutmeg => nutmeg_compiler;

define plant_expr( expr );
    if expr.isConstant then
        sysPUSHQ( expr.valueConstant )
    elseif expr.isId then
        lvars idref = resolve( expr );
        sysPUSHQ( idref );
        sysFIELD( 1, IdRef_key.class_spec, false );
        sysPUSHQ( [id ^(expr.nameId)] )
    elseif expr.isSeq then
        appdata( expr, plant_expr )
    elseif expr.isApply then
        dlocal pop_new_lvar_list;
        lvars stack_count = sysNEW_LVAR();
        sysCALL( "stacklength" );
        sysPOP( stack_count );
        plant_expr( expr.argsApply );
        sysCALL( "stacklength" );
        sysPUSH( "stack_count" );
        sysCALL( "fi_-" );
        plant_expr( expr.fnApply );
        sysCALLS( _ );
    else
        mishap( 'Do not know how to compile this', [ ^expr ] )
    endif;
    sysPUSHQ( true );
    sysCALL( "sysprarrow" );
enddefine;

;;;
;;; Here we use -proglist_state- and -proglist_new_state-, even though it is
;;; overkill, to convert various kinds of source into a character repeater.
;;; We will actually override proglist so that it becomes a dynamic list of
;;; character codes.
;;;
define procedure nutmeg_compiler( source );
    dlocal proglist_state = proglist_new_state(source);
    procedure();
        until null(proglist) do
            lvars e = read_ne_expr();
            plant_expr( e );
            sysEXECUTE();
        enduntil;
    endprocedure.sysCOMPILE;
enddefine;

endsection;

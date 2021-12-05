compile_mode :pop11 +strict;

section $-nutmeg => nutmeg_compiler;

vars procedure prefix_table =
    newanyproperty(
        [], 8, 1, false,
        false, false, "perm",
        false, false
    );

vars procedure postfix_table =
    newanyproperty(
        [], 8, 1, false,
        false, false, "perm",
        false, false
    );

lconstant punctuation_list = [ , ; ) ];
vars procedure punctuation_table =
    newanyproperty(
        maplist( punctuation_list, procedure( w ); [ ^w true ] endprocedure ),
        round( length( punctuation_list ) * 1.5 ),
        false, false,
        false, false, "perm",
        false, false
    );

define peekitem();
    dlocal proglist;
    readitem()
enddefine;

define try_read_expr();
    lvars item = peekitem();
    if item == termin or punctuation_table( item ) then
        false
    else
        proglist.destpair -> proglist -> item;
        if item.isstring or item.isnumber then
            consConstant( item )
        else
            lvars mini_parser = prefix_table( item );
            if mini_parser then
                mini_parser()
            else
                consId( item )
            endif
        endif
    endif
enddefine;

define read_ne_expr() -> e;
    lvars e = try_read_expr();
    unless e do
        lvars item = readitem();
        if item == termin then
            mishap( 'Unexpected end of input', [ ^item ] )
        else
            mishap( 'Unexpected item (missing expression?)', [ ^item ] )
        endif
    endunless;
enddefine;

define read_seq_ne_expr();
    newSeq(#|
        repeat
            lvars e = try_read_expr();
            quitunless( e );
            e;
            quitunless( pop11_try_nextreaditem([, ;]) )
        endrepeat
    |#)
enddefine;

define read_stmnt();
    consSeq(#|
        repeat
            read_expr();
            quitunless( pop11_try_nextreaditem([, ;]) )
        endrepeat
    |#)
enddefine;


define parenthesis_prefix_parser();
    read_seq_ne_expr();
    pop11_need_nextreaditem(")") -> _;
enddefine;
parenthesis_prefix_parser -> prefix_table( "(" );

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

define plant_expr( expr );
    if expr.isConstant then
        sysPUSHQ( expr.valueConstant )
    elseif expr.isId then
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

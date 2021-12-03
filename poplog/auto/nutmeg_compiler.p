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

lconstant punctuation_list = [ , ; ];
vars procedure punctuation_table =
    newanyproperty(
        maplist( punctuation_list, procedure( w ); [ ^w true ] endprocedure ),
        round( length( punctuation_list ) * 1.5 ),
        false, false,
        false, false, "perm",
        false, false
    );

define read_expr();
    lvars item = readitem();
    if item == termin or punctuation_table( item ) then
        mishap( 'Unexpected end of input', [ ^item ] )
    elseif item.isstring or item.isnumber then
        [constant ^item]
    else
        lvars mini_parser = prefix_table( item );
        if mini_parser then
            mini_parser()
        else
            [id ^item]
        endif
    endif;
enddefine;

define read_stmnt();
    [%
        repeat
            read_expr();
            quitunless( pop11_try_nextreaditem([, ;]) )
        endrepeat
    %]
enddefine;

define plant_expr( expr );
    sysPUSHQ( expr );
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
            lvars e = read_expr();
            plant_expr( e );
            sysEXECUTE();
        enduntil;
    endprocedure.sysCOMPILE;
enddefine;

endsection;

;;; -- Adding a new language -----------------------------------------------

uses subsystem;

;;; Correct long-standing bug in subsystem
if subscr_subsystem( 4, "pop11" ) = ':  ' then
    ': ' -> subscr_subsystem( 4, "pop11" )
endif;

define lconstant procedure subsystem_exists( name ); lvars name;
    lvars ss;
    for ss in sys_subsystem_table do
        if hd( ss ) = name then
            return( true )
        endif
    endfor;
    return( false );
enddefine;

define set_ved_compiler( file_ext, comp ); lvars file_ext, comp;
    lvars i, v;
    for i in vedfiletypes do
        if i.islist
        and i(1) = file_ext
        and (i(2) ->> v).isvector
        and v.length = 2
        and v(1) = "popcompiler"
        then
            comp -> v( 2 );         ;;; in-place update of vedfiletypes!
            return;
        endif;
    endfor;
    [[^file_ext {popcompiler ^comp}] ^^vedfiletypes] -> vedfiletypes;
enddefine;

define lconstant procedure set_subsystem( name, comp, file_ext, prompt, reset );
    lvars name, comp, file_ext, prompt, reset;
    if subsystem_exists( name ) then
        comp        -> subscr_subsystem( 2, name );
        file_ext    -> subscr_subsystem( 3, name );
        prompt      -> subscr_subsystem( 4, name );
        reset       -> subscr_subsystem( 5, name );
    else
        [[^name ^comp ^file_ext ^prompt ^reset] ^^sys_subsystem_table ] -> sys_subsystem_table;
    endif;
    set_ved_compiler( file_ext, comp );
    unless member( file_ext, vednonbreakfiles ) do
        [^file_ext ^^vednonbreakfiles] -> vednonbreakfiles;
    endunless;
enddefine;

define lconstant procedure is_in_ved_im();
    lvars n;
    (iscaller( vedsetpop ) ->> n) and
    pdprops( caller( 1 + n ) ) == "start_im"
enddefine;

define addlanguage( d ); lvars d;
    lvars name, comp, topname, top, file_ext, prompt, reset;

    ;;; WARNING : lib subsystem asks us to always call cucharin!

    define lvars procedure i_call_comp( cucharin ); dlocal cucharin;
        dlocal popprompt = prompt;
        if is_in_ved_im() then
            top
        else
            comp
        endif( cucharin )
    enddefine;

    define lvars procedure i_call_top( cucharin ); dlocal cucharin;
        dlocal popprompt = prompt;
        top( cucharin )
    enddefine;

    if d.islist then d.assoc -> d endif;
    applist(
        [name top_level_name compiler top_level file_ext prompt reset],
        d
    ) -> reset -> prompt -> file_ext -> top -> comp -> topname -> name;

    if top and not( topname ) then "top_" <> name -> topname endif;
    unless top then comp -> top endunless;
    unless file_ext do '.' sys_>< name -> file_ext endunless;
    unless prompt do popprompt -> prompt endunless;
    unless reset do npr(% '\nSet ' sys_>< name %) -> reset endunless;

    set_subsystem( name, i_call_comp, file_ext, prompt, reset );

    if topname then
        set_subsystem( topname, i_call_top, file_ext, prompt, reset );
    endif;
enddefine;

define macro language( name ); lvars name;
    name -> subsystem;
    switch_subsystem_to( name );
    lvars file_ext = subscr_subsystem( 3, name );
    'output' <> file_ext -> vedlmr_print_in_file;
enddefine;

;;; -------------------------------------------------------------------------

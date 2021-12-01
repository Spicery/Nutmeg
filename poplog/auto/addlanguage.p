compile_mode :pop11 +strict;
section;

include subsystem.ph;


;;; -- Adding a new language -----------------------------------------------

uses subsystem;

define lconstant procedure subsystem_exists( name ); lvars name;
    lvars ss;
    for ss in sys_subsystem_table do
        if hd( ss ) = name then
            return( true )
        endif
    endfor;
    return( false );
enddefine;

define set_ved_compiler( file_ext, name ); lvars file_ext, name;
    lvars i, v;
    for i in vedfiletypes do
        if i.islist
        and i(1) = file_ext
        and (i(2) ->> v).isvector
        and v.length = 2
        and v(1) = "subsystem"
        then
            name -> v( 3 );         ;;; in-place update of vedfiletypes!
            return;
        endif;
    endfor;
    [[^file_ext {subsystem " ^name}] ^^vedfiletypes] -> vedfiletypes;
enddefine;

define lconstant procedure set_subsystem( name, comp, file_ext, prompt, reset );
    lvars name, comp, file_ext, prompt, reset;
    if subsystem_exists( name ) then
        comp        -> subscr_subsystem( SS_COMPILER, name );
        file_ext    -> subscr_subsystem( SS_FILE_EXTN, name );
        prompt      -> subscr_subsystem( SS_PROMPT, name );
        reset       -> subscr_subsystem( SS_RESET, name );
    else
        subsystem_add_new( name, { ^comp ^reset }, file_ext, prompt, [], name.word_string );
    endif;
    set_ved_compiler( file_ext, name );
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

    define lvars procedure i_call_comp( source );
        dlocal popprompt = prompt;
        if is_in_ved_im() then
            top
        else
            comp
        endif( source )
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

endsection;

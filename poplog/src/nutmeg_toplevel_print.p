compile_mode :pop11 +strict;

section;

define global nutmeg_toplevel_print();
    dlocal pop_pr_quotes = true;
    if stacklength() > 0 then
        sysprarrow( true )
    endif
enddefine;

endsection;

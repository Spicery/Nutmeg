compile_mode :pop11 +strict;

section;

lvars nutmeg_directory = sys_fname_path( popfilename );

extend_searchlist(
    nutmeg_directory dir_>< 'auto',
    popautolist
) -> popautolist;

extend_searchlist(
    nutmeg_directory dir_>< 'lib',
    popuseslist
) -> popuseslist;

extend_searchlist(
    nutmeg_directory dir_>< 'help',
    vedhelplist
) -> vedhelplist;

uses addlanguage

define procedure nutmeg_compiler( cucharin );
    dlocal cucharin;
    lvars ch;
    for ch from_repeater cucharin do
        ch =>
    endfor
enddefine;

vars procedure nutmeg_reset = identfn;


[
    [ name       nutmeg ]
    [ compiler   ^nutmeg_compiler ]
    [ file_ext   '.nutmeg' ]
    [ prompt     'o-/-\-> ' ]
    [ reset      ^nutmeg_reset ]
].addlanguage;


endsection;

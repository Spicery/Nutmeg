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
    nutmeg_directory dir_>< 'src',
    vedsrclist
) -> vedsrclist;

extend_searchlist(
    nutmeg_directory dir_>< 'help',
    vedhelplist
) -> vedhelplist;

loadlib( "nuttree", popuseslist );
loadlib( "nutmeg_parse", popuseslist );
loadlib( "nutmeg_arity", popuseslist );
loadlib( "nutmeg_resolve", popuseslist );
loadlib( "nutmeg_compiler", popuseslist );
loadlib( "nutmeg_reset", popuseslist );

uses addlanguage

[
    [ name       nutmeg ]
    [ compiler   ^(procedure() with_nargs 1; nutmeg_compiler() endprocedure) ]
    [ file_ext   '.nutmeg' ]
    [ prompt     'o-/-\-> ' ]
    [ reset      ^(procedure(); nutmeg_reset() endprocedure) ]
].addlanguage;

endsection;

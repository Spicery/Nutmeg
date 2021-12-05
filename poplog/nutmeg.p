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

uses nuttree
uses read_expr
uses arity
uses nutmeg_compiler
uses nutmeg_reset

uses addlanguage

[
    [ name       nutmeg ]
    [ compiler   ^(procedure() with_nargs 1; nutmeg_compiler() endprocedure) ]
    [ file_ext   '.nutmeg' ]
    [ prompt     'o-/-\-> ' ]
    [ reset      ^(procedure(); nutmeg_reset() endprocedure) ]
].addlanguage;


endsection;

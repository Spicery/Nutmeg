compile_mode :pop11 +strict;

uses dict

section $-nutmeg;

define add_info( dict, p ) -> p;
    if dict.isprocedure then
        ${
            arity = newExactArity( dict.pdnargs ),
            arityChecked = dict
        } -> dict
    endif;
    if p.pdprops.ispair and p.pdprops.back.isdict then
        mishap( 'Internal error', [ ^dict ^p ] )
    endif;
    conspair( p.pdprops, dict ) -> p.pdprops;
enddefine;

defclass NutmegDatakey {
    datakeyNutmegDatakey
};
procedure( N, nk );
    lvars key = datakeyNutmegDatakey( nk );
    lvars len = key.datalength;
    if N == len then
        class_cons( datakeyNutmegDatakey( nk ) )
    else
        lvars args = conslist( N );
        mishap( 'Argument mismatch (expecting ' >< len >< ')', args );
    endif
endprocedure -> class_apply( NutmegDatakey_key );


defclass FilePath {
    pathFilePath
};

add_info(
    consFilePath,
    procedure( N ) with_props FilePath;
        if N == 1 then
            consFilePath()
        else
            lvars args = conslist( N );
            mishap( 'Argument mismatch (expecting one argument)', args );
        endif
    endprocedure
) -> nutmeg_valof( "FilePath" );

endsection;

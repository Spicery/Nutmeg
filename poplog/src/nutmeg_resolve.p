compile_mode :pop11 +strict;

section $-nutmeg => nutmeg_valof;


defclass IdRef {
    contIdRef
};

vars procedure nutmeg_packages =
    newanyproperty(
        [], 8, 1, false,
        false, false, false,
        false, false
    );

define declare_name( name );
    lvars idref = nutmeg_packages( name );
    if idref then
        idref
    else
        consIdRef( _ ) ->> nutmeg_packages( name );
    endif
enddefine;

define resolve_name( name );
    lvars idref = nutmeg_packages( name );
    if idref then
        idref
    else
        mishap( 'Unknown identifier', [ ^name ] )
    endif
enddefine;

define resolve( id );
    id.nameId.resolve_name
enddefine;

define nutmeg_valof( w );
    contIdRef( resolve_name( w ) )
enddefine;

define updaterof nutmeg_valof( v, w );
    v -> contIdRef( declare_name( w ) )
enddefine;

endsection;

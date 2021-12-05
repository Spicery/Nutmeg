compile_mode :pop11 +strict;

section $-nutmeg;


defclass IdRef {
    valueIdRef
};

vars procedure nutmeg_packages =
    newanyproperty(
        [], 8, 1, false,
        false, false, false,
        false, false
    );

define resolve( id );
    lvars name = id.nameId;
    lvars idref = nutmeg_packages( name );
    if idref then
        idref
    else
        mishap( 'Unknown identifier', [ ^name ] )
    endif
enddefine;

endsection;

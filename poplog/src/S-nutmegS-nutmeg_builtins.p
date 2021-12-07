compile_mode :pop11 +strict;

uses dict
uses $-nutmeg$-nutmeg_resolve;
uses $-nutmeg$-nutmeg_arity;
uses $-nutmeg$-arity_expr;

section $-nutmeg;
vars nutmeg_builtins = _;

define check_exact_arity( procedure p, count ) -> closure;
    procedure( N, p, count );
        if N == count then
            fast_chain( p )
        else
            lvars args = conslist( N );
            mishap( 'Argument mismatch (expecting ' >< count >< ')', args );
        endif
    endprocedure(% p, count %) -> closure;
    p.pdprops -> closure.pdprops;
enddefine;

define add_info( dict, p ) -> p;
    if dict.isprocedure then
        unless p do
            check_exact_arity( dict, dict.pdnargs ) -> p
        endunless;
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

define has_exact_arity( p, count );
    lvars d = p.pdprops.back;
    lvars a = d( "arity" );
    if a.countArity == count and a.isExactArity then
        d( "arityChecked" )
    else    
        false
    endif
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

;;; --- ReadLines

;;; TODO: this definition is almost certainly incorrect. We do not want to expand the list in general.
define ReadLines( filepath );
    discinline( filepath.pathFilePath ).pdtolist.expandlist
enddefine;
add_info( ReadLines, false ) -> nutmeg_valof( "ReadLines" );

;;; --- ToInteger

define ToInteger( str );
    lvars n = strnumber( str );
    if n.isintegral then
        n
    else    
        mishap( 'String is not an integer (radix 10)', [ ^str ] )
    endif
enddefine;
add_info( ToInteger, false ) -> nutmeg_valof( "ToInteger" );

;;; --- Map

;;; TODO: This is almost certainly incorrect. We need Map to work over anything of type Series.
define Map( procedure fn, list );
    lvars p = has_exact_arity( fn, 1 );
    if p then
        maplist( list, p )
    else
        [%
            lvars i;
            for i in list do
                fn( i, 1 )
            endfor
        %]
    endif
enddefine;
add_info( Map, false ) -> nutmeg_valof( "Map" );



endsection;


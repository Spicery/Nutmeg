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
    if p.pdprops.ispair and p.pdprops.back.isdict then
        mishap( 'Internal error', [ ^dict ^p ] )
    endif;
    conspair( p.pdprops, dict ) -> p.pdprops;
enddefine;

define to_nutproc_from_both( uncounted_p, counted_p ) -> p;
    lvars dict =
        ${
            arity = newExactArity( uncounted_p.pdnargs ),
            arityChecked = uncounted_p
        };
    add_info( dict, counted_p ) -> p;
enddefine;

define to_nutproc_from_uncounted( d ) -> p;
    lvars p = check_exact_arity( d, d.pdnargs );
    lvars dict =
        ${
            arity = newExactArity( d.pdnargs ),
            arityChecked = d
        };
    add_info( dict, p ) -> p;
enddefine;

define to_nutproc_from_counted( p, n ) -> p;
    lvars dict =
        ${
            arity = newInexactArity( n )
        };
    add_info( dict, p ) -> p;
enddefine;

define has_exact_arity( p, count );
    returnunless( p.pdprops.ispair )( false );
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

to_nutproc_from_both(
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
to_nutproc_from_uncounted( ReadLines ) -> nutmeg_valof( "ReadLines" );

;;; --- ToInteger

define ToInteger( str );
    lvars n = strnumber( str );
    if n.isintegral then
        n
    else
        mishap( 'String is not an integer (radix 10)', [ ^str ] )
    endif
enddefine;
to_nutproc_from_uncounted( ToInteger ) -> nutmeg_valof( "ToInteger" );

;;; --- Length -----------------------------------------------------------------

;;; TODO: This is almost certainly incorrect. We need Length to work over anything of type Series.
to_nutproc_from_uncounted( length ) -> nutmeg_valof( "Length" );

;;; --- Select -----------------------------------------------------------------

;;; TODO: This is almost certainly incorrect. We need Select to work over anything of type Series.
;;; TODO: Got to handle out-arity /= 1
define Select( list, procedure fn );
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
to_nutproc_from_uncounted( Select ) -> nutmeg_valof( "Select" );

;;; --- Where ------------------------------------------------------------------

;;; TODO: Got to handle out-arity /= 1
define Where( list, procedure fn );
    lvars p = has_exact_arity( fn, 1 );
    if p then
        [%
            lvars i;
            for i in list do
                if p( i ) then i endif
            endfor
        %]
    else
        [%
            lvars i;
            for i in list do
                if fn( i, 1 ) then i endif
            endfor
        %]
    endif
enddefine;
to_nutproc_from_uncounted( Where ) -> nutmeg_valof( "Where" );

;;; --- Zip --------------------------------------------------------------------

define Zip( N );
    if N == 3 then
        lvars ( procedure p, list1, list2 ) = ();
        [%
            lvars i, j;
            for i, j in list1, list2 do
                p( i, j, 2 )
            endfor
        %]
    elseif N == 2 then
        lvars (procedure p, list) = ();
        Select( list, p )
    elseif N == 0 then
        ;;; Skip.
    elseif N >= 1 then
        lvars L = N - 1;
        lvars lists = conslist( L );
        lvars procedure p = ();
        [%
            repeat
                lvars a;
                for a on lists do
                    if a.front.null do
                        _;              ;;; to ensure stack balances
                        erasenum -> p;  ;;; force cleanup! And signal end of processing.
                    else
                        a.front.destpair -> a.front
                    endif
                endfor;
                p( L );                     ;;; call the procedure (or erasenum)
                quitif( p == erasenum );    ;;; break if any list has exhausted
            endrepeat
        %]
    else
        mishap( 'No arguments to Zip', [] )
    endif
enddefine;
to_nutproc_from_counted( Zip, 1 ) -> nutmeg_valof( "Zip" );

;;; --- Split ------------------------------------------------------------------

define Split( string );
    split_by_spaces( string, false, conslist )
enddefine;
to_nutproc_from_uncounted( Split ) -> nutmeg_valof( "Split" );

;;; --- Head, Tail & IsEmpty ---------------------------------------------------
;;; Consider First, Rest and Last. IsEmpty is fine, I think.

;;; TODO: The name is incorrect
to_nutproc_from_uncounted( tl ) -> nutmeg_valof( "Tail" );
to_nutproc_from_uncounted( hd ) -> nutmeg_valof( "Head" );
to_nutproc_from_uncounted( null ) -> nutmeg_valof( "IsEmpty" );

;;; --- Arithmetic -------------------------------------------------------------

;;; TODO: the name is incorrect
to_nutproc_from_uncounted( nonop - ) -> nutmeg_valof( "-" );
to_nutproc_from_uncounted( nonop + ) -> nutmeg_valof( "+" );
to_nutproc_from_uncounted( nonop / ) -> nutmeg_valof( "/" );
to_nutproc_from_uncounted( nonop * ) -> nutmeg_valof( "*" );

define SumOf( N );
    0;
    repeat N times
        nonop +()
    endrepeat
enddefine;
to_nutproc_from_counted( SumOf, 0 ) -> nutmeg_valof( "SumOf" );

define Sum( L );
    if L.islist then 
        applist( 0, L, nonop + )
    else
        mishap( 'Series needed', [^L] )
    endif
enddefine;
to_nutproc_from_uncounted( Sum ) -> nutmeg_valof( "Sum" );

;;; --- Comparison -------------------------------------------------------------

to_nutproc_from_uncounted( nonop < ) -> nutmeg_valof( "<" );
to_nutproc_from_uncounted( nonop = ) -> nutmeg_valof( "==" );


;;; --- Sequences --------------------------------------------------------------

define Seq( N );
    conslist( N )
enddefine;
to_nutproc_from_counted( Seq, 0 ) -> nutmeg_valof( "Seq" );

;;; --- Print ------------------------------------------------------------------

define Print( N );
    lvars L = conslist( N );
    lvars i, first = true;
    for i in L do
        if first then
            false -> first
        else
            cucharout( ' ' )
        endif;
        nutmeg_print( i )
    endfor;
enddefine;
to_nutproc_from_counted( Print, 0 ) -> nutmeg_valof( "Print" );

endsection;

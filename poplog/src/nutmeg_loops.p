compile_mode :pop11 +strict;

uses characters;

section $-nutmeg => nutmeg_initialise_loop;

/*
    The pattern for loops is that there is a different advancer for each type of
    object to iterate over. The advancer takes two inputs and delivers either
    three results or a single result.

        advancer( that_which_changes, that_that_stays_the_same ) ->
            ( false )
            ( the_next_value, the_value_to_pattern_match, true )

    The nutmeg_initialise_loop sets this up:

        nutmeg_initialise_loop( object ) ->
            ( advancer, that_which_changes, that_that_stays_the_same )
*/

;;; return( advancer, that_which_changes, that_that_stays_the_same )
define global nutmeg_initialise_loop( item );
    if item.islist then
        procedure( L, _ );
            if null( L ) then
                false
            else
                ( fast_destpair( L ), true )
            endif
        endprocedure,
        item,
        termin
    elseif item.isstring then
        procedure( n, S );
            if n <= datalength(S) then
                n fi_+ 1,
                newCharacter( fast_subscrs( n, S ) );
                true;
            else
                false
            endif
        endprocedure,
        1,
        item
    else
        mishap( 'Cannot iterate over this object', [ ^item ] )
    endif
enddefine;

endsection;

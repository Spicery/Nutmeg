compile_mode :pop11 +strict;

section $-nutmeg;

defclass Character { codeCharacter : 32 };

lconstant SIZE = 256;
lconstant CACHE = initv( SIZE );

define newCharacter( n );
    if n.isinteger then
        if 0 <= n and n < SIZE then
            lvars ch = fast_subscrv( n fi_+ 1, CACHE );
            if ch /== undef then
                ch
            else
                consCharacter( n ) ->> fast_subscrv( n fi_+ 1, CACHE )
            endif
        else
            consCharacter( n )
        endif
    else
        mishap( 'Integer expected', [ ^n ] )
    endif
enddefine;


endsection;

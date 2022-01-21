compile_mode :pop11 +strict;

section $-nutmeg => nutmeg_tokeniser;

uses newpushable;

define constant procedure iswhitespacecode( ch );
    lvars white_chars = popnewline and '\s\t' or '\s\t\r\n';
    ch.isinteger and locchar( ch, 1, white_chars )
enddefine;

define constant procedure ispunccode( ch );
    ch.isinteger and locchar( ch, 1, '()[]{};,\\' )
enddefine;

define constant procedure issigncode( ch );
    ch.isinteger and locchar( ch, 1, '+-*/=!$%^&|?.:<>' )
enddefine;

define constant procedure peek( charsrc );
    charsrc() ->> charsrc()
enddefine;

define constant read_identifier( charsrc, ch );
    lvars procedure charsrc;
    consword(#|
        ch;
        charsrc() -> ch;
        while ch.isalphacode or ch.isnumbercode or ch == `_` do
            ch;
            charsrc() -> ch
        endwhile;
        ch -> charsrc();
    |#)
enddefine;

define constant read_number( charsrc, _sign );    
    lvars procedure charsrc;
    lvars s = (
        consstring(#|
            lvars ch = charsrc();
            while ch.isnumbercode do
                ch;
                charsrc() -> ch;
            endwhile;
            ch -> charsrc();
        |#)
    );
    lvars n = s.strnumber;
    unless n do
        mishap( 'Invalid number syntax', [ ^s ] )
    endunless;
    return( _sign * n )
enddefine;

define constant read_string( charsrc, quote_ch );
    lvars procedure charsrc;
    lvars ok = true;
    lvars s = (
        consstring(#|
            repeat
                lvars ch = charsrc();
                quitif( ch == quote_ch );
                if ch == `\n` or ch == `\r` then
                    false -> ok;
                    quitloop
                else
                    ch;
                endif;
            endrepeat;
        |#)
    );
    unless ok do
        mishap( 'Unexpected end of line found before end of string', [ ^s ] )
    endunless;
    return( s );
enddefine;

;;; This function looks ahead to see if the next characters in the newpushable
;;; repeater matches the match_string. If they do, then the characters are 
;;; consumed. Otherwise any characters read are pushed back in the right order.
define try_read( charsrc, string ) -> result;
    lvars result = true;
    lvars buffer = [];
    lvars ch;
    for ch in_string string do
        lvars nch = charsrc();
        conspair( nch, buffer ) -> buffer;
        ( nch == ch ) and result -> result;
        quitunless( result );
    endfor;
    if result then
        sys_grbg_list( buffer )
    else
        while buffer.ispair do
            sys_grbg_destpair( buffer ) -> buffer -> charsrc()
        endwhile;
    endif
enddefine;

define read_eol_comment( charsrc );
    lvars procedure charsrc;
    repeat
        lvars ch = charsrc();
        if ch == termin then
            ;;; The underlying repeater is not guaranteed to continue to 
            ;;; produce termins. This is a hidden fact in Poplog, see HELP
            ;;; INCHARITEM, Using incharitem with discin, which appears to be
            ;;; the only place it is documented.
            ch -> charsrc();
            return;
        endif;
        returnif( ch == `\n` );
        if ch == `\r` then
            try_read( charsrc, '\n' );
            charsrc() -> ch;
            return;
        endif;
    endrepeat
enddefine;

define constant procedure read_sign_identifier( charsrc, ch );
    lvars procedure charsrc;
    consword(#|
        ch;
        repeat
            charsrc() -> ch;
            unless ch.issigncode do
                ch -> charsrc();
                quitloop
            endunless;
            ch;
        endrepeat
    |#)
enddefine;

define tokeniser( charsrc );
    lvars procedure charsrc;

    lvars ch = charsrc();
    while iswhitespacecode( ch ) do
        charsrc() -> ch
    endwhile;
    returnif( ch == termin )( termin );
    if ch.isalphacode or ch == `_` then
        read_identifier( charsrc, ch )
    elseif ch.isnumbercode then
        ch -> charsrc();
        read_number( charsrc, 1 )
    elseif ( ch == `+` or ch == `-` ) and peek( charsrc ).isnumbercode then
        read_number( charsrc, ch == `-` and -1 or 1 )
    elseif ch == `'` or ch == `"` then
        read_string( charsrc, ch )
    elseif ch == ``` then
        lvars s = read_string( charsrc, ch );
        if s.datalength == 1 then
            newCharacter( subscrs( 1, s ) )
        else
            mishap( 'Invalid character syntax (must be a single character)', [ ^s ] )
        endif
    elseif ch == `#` and try_read( charsrc, '##' ) then
        read_eol_comment( charsrc );    ;;; returns no results.
        chain( charsrc, tokeniser );    ;;; tail-call optimised loop.
    elseif ch.ispunccode then
        consword( ch, 1 )
    elseif ch.issigncode then
        read_sign_identifier( charsrc, ch )
    elseif ch == `\n` then
        newline
    elseif ch == `\r` then
        try_read( charsrc, '\n' );
        newline
    else
        mishap( 'Unexpected character', [% consstring( ch, 1 ) %] )
    endif
enddefine;

define nutmeg_tokeniser( repeater );
    tokeniser(% newpushable( repeater ) %)
enddefine;

endsection;
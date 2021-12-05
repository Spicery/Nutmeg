compile_mode :pop11 +strict;

section $-nutmeg;

vars procedure prefix_table =
    newanyproperty(
        [], 8, 1, false,
        false, false, "perm",
        false, false
    );

vars procedure postfix_table =
    newanyproperty(
        [], 8, 1, false,
        false, false, "perm",
        false, false
    );

lconstant punctuation_list = [ , ; ) ];
vars procedure punctuation_table =
    newanyproperty(
        maplist( punctuation_list, procedure( w ); [ ^w true ] endprocedure ),
        round( length( punctuation_list ) * 1.5 ),
        false, false,
        false, false, "perm",
        false, false
    );

define peekitem();
    dlocal proglist;
    readitem()
enddefine;

define try_read_expr();
    lvars item = peekitem();
    if item == termin or punctuation_table( item ) then
        false
    else
        proglist.destpair -> proglist -> item;
        if item.isstring or item.isnumber then
            consConstant( item )
        else
            lvars mini_parser = prefix_table( item );
            if mini_parser then
                mini_parser()
            else
                consId( item )
            endif
        endif
    endif
enddefine;

define read_ne_expr() -> e;
    lvars e = try_read_expr();
    unless e do
        lvars item = readitem();
        if item == termin then
            mishap( 'Unexpected end of input', [ ^item ] )
        else
            mishap( 'Unexpected item (missing expression?)', [ ^item ] )
        endif
    endunless;
enddefine;

define read_seq_ne_expr();
    newSeq(#|
        repeat
            lvars e = try_read_expr();
            quitunless( e );
            e;
            quitunless( pop11_try_nextreaditem([, ;]) )
        endrepeat
    |#)
enddefine;

define parenthesis_prefix_parser();
    read_seq_ne_expr();
    pop11_need_nextreaditem(")") -> _;
enddefine;
parenthesis_prefix_parser -> prefix_table( "(" );

endsection;

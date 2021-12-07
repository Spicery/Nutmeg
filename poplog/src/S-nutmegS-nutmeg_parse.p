compile_mode :pop11 +strict;

uses int_parameters

uses $-nutmeg$-newSingleValue;

section $-nutmeg;
vars nutmeg_parse = _;

vars procedure prefix_table =
    newanyproperty(
        [], 8, 1, false,
        false, false, "perm",
        false, false
    );

defclass PostfixEntry {
    precedencePostfixEntry,
    miniParserPostfixEntry
};

vars procedure postfix_table =
    newanyproperty(
        [], 8, 1, false,
        false, false, "perm",
        false, false
    );

lconstant punctuation_list = [ , ; ) ^newline end enddef ];
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

define try_read_expr_prec( prec ) -> sofar;
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
                newId( item )
            endif
        endif
    endif -> sofar;
    repeat
        peekitem() -> item;
        lvars postfix_entry = postfix_table( item );
        quitunless( postfix_entry );
        lvars p = postfix_entry.precedencePostfixEntry;
        quitif( p > prec );
        proglist.back -> proglist;
        miniParserPostfixEntry( postfix_entry )( p, sofar, item ) -> sofar
    endrepeat;
enddefine;

define read_optexpr() -> e;
    try_read_expr_prec( pop_max_int ) -> e
enddefine;

define read_expr() -> e;
    lvars e = read_optexpr();
    unless e do
        lvars item = readitem();
        if item == termin then
            mishap( 'Unexpected end of input', [ ^item ] )
        else
            mishap( 'Unexpected item (missing expression?)', [ ^item ] )
        endif
    endunless;
enddefine;

define read_expr_seq();
    newSeq(#|
        repeat
            lvars e = try_read_expr_prec( pop_max_int );
            quitunless( e );
            e;
            quitunless( pop11_try_nextreaditem([, ; ^newline]) )
        endrepeat
    |#)
enddefine;

define read_optexpr_seq();
    newSeq(#|
        repeat
            lvars e = try_read_expr_prec( pop_max_int );
            if e then e endif;
            quitunless( pop11_try_nextreaditem([, ; ^newline]) )
        endrepeat
    |#)
enddefine;

define read_stmnt_seq(popnewline);
    dlocal popnewline;
    read_optexpr_seq()
enddefine;

;;; -- def --------------------------------------------------------------------

define def_prefix_parser();
    lvars template = read_expr();
    pop11_need_nextreaditem( ":" ) -> _;
    lvars stmnts = read_stmnt_seq( true );
    pop11_need_nextreaditem( [end enddef] ) -> _;
    if template.isApply then
        lvars fn = template.fnApply;
        lvars args = template.argsApply;
        if fn.isId then
            newBind( fn, newFn( fn.nameId, args, stmnts ) )
        else
            mishap( 'Invalid function header', [% fn %] )
        endif
    elseif template.isId then
        newBind( template, newSingleValue( stmnts ) )
    else
        mishap( 'Invalid def header', [ ^fn ] )
    endif
enddefine;
def_prefix_parser -> prefix_table( "def" );


;;; -- (-----------------------------------------------------------------------

define parenthesis_prefix_parser();
    dlocal popnewline = false;
    read_expr_seq();
    pop11_need_nextreaditem( ")" ) -> _;
enddefine;
parenthesis_prefix_parser -> prefix_table( "(" );

define parenthesis_postfix_parser( prec, lhs, token );
    dlocal popnewline = false;;
    lvars rhs = read_expr_seq();
    pop11_need_nextreaditem( ")" ) -> _;
    newApply( lhs, rhs )
enddefine;
consPostfixEntry( 10, parenthesis_postfix_parser ) -> postfix_table( "(" );

;;; -- . -----------------------------------------------------------------------

define is_ordinary( word );
    returnunless( word.isword )( false );
    returnif( word.punctuation_table )( false );
    returnif( word.postfix_table )( false );
    returnif( word.prefix_table )( false );
    true
enddefine;

define dot_postfix_parser( prec, lhs, token );
    dlocal popnewline;
    lvars name = readitem();
    unless name.is_ordinary then    
        mishap( 'Unexpected token following dot (.)', [ ^name ] )
    endunless; 
    if pop11_try_nextreaditem( "(" ) then
        false -> popnewline;
        lvars rhs = read_expr_seq();
        pop11_need_nextreaditem( ")" ) -> _;
        newApply( newId( name ), newSeq(#| lhs, rhs |#) )
    else
        newApply( newId( name ), lhs )
    endif
enddefine;
consPostfixEntry( 11, dot_postfix_parser ) -> postfix_table( "." );

;;; -- := ----------------------------------------------------------------------

define bind_postfix_parser( prec, lhs, token );
    lvars rhs = try_read_expr_prec( prec );
    newBind( lhs, rhs )
enddefine;
consPostfixEntry( 990, bind_postfix_parser ) -> postfix_table( ":=" );


endsection;

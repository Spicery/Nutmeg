compile_mode :pop11 +strict;

uses int_parameters

uses $-nutmeg$-newSingleValue;
uses $-nutmeg$-replace_holes;

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

lconstant punctuation_list = [ , ; ) ^newline end enddef endswitch case then else ];
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
            elseif punctuation_table( item ) then
                mishap( 'Unexpected punctation while reading expression', [ ^item ] )
            elseif postfix_table( item ) then
                mishap( 'Missing expression before operator/keyword', [ ^item ] )
            else
                newId( item )
            endif
        endif
    endif -> sofar;
    repeat
        peekitem() -> item;
        ;;; TODO add special case for negative integer.
        lvars postfix_entry = postfix_table( item );
        quitunless( postfix_entry );
        lvars p = postfix_entry.precedencePostfixEntry;
        quitif( p > prec );
        proglist.back -> proglist;
        miniParserPostfixEntry( postfix_entry )( p, sofar, item ) -> sofar
    endrepeat;
enddefine;

define read_expr_prec( prec ) -> e;
    lvars e = try_read_expr_prec( prec );
    unless e do
        lvars item = readitem();
        if item == termin then
            mishap( 'Unexpected end of input', [ ^item ] )
        else
            mishap( 'Unexpected item (missing expression?)', [ ^item ] )
        endif
    endunless;
enddefine;

define read_optexpr() -> e;
    try_read_expr_prec( pop_max_int ) -> e
enddefine;

define read_expr() -> e;
    read_expr_prec( pop_max_int ) -> e
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

define read_optexpr_seq_helper() -> ( expr, non_empty );
    false -> non_empty;
    newSeq(#|
        repeat
            lvars e = try_read_expr_prec( pop_max_int );
            if e then 
                true -> non_empty;
                e 
            endif;
            quitunless( pop11_try_nextreaditem([, ; ^newline]) )
        endrepeat
    |#) -> expr
enddefine;

define read_optexpr_seq();
    read_optexpr_seq_helper().erase
enddefine;

define read_stmnt_seq(popnewline);
    dlocal popnewline;
    lvars ( expr, non_empty ) = read_optexpr_seq_helper();
    if non_empty then
        expr
    else
        mishap( 'Missing statements', [] )
    endif
enddefine;

;;; --- handy constants

lconstant end_list = [end];

;;; --- var --------------------------------------------------------------------

define read_variable_name() -> item;
    readitem() -> item;
    if item == termin or item.isstring or item.isnumber or punctuation_table( item ) then
        mishap( 'Variable name required', [ ^item ] )
    elseif prefix_table( item ) or punctuation_table( item ) or postfix_table( item ) do
        mishap( 'Syntax found while looking for variable name', [ ^item ] )
    endif
enddefine;

define vaX_prefix_parser( assignable ) -> id;
    lvars w = read_variable_name();
    newId( w ) -> id;
    assignable -> id.isAssignableId;
enddefine;

vaX_prefix_parser(% true %) -> prefix_table( "var" );
vaX_prefix_parser(% false %) -> prefix_table( "val" );


;;; --- def --------------------------------------------------------------------


lconstant def_end_list = [ enddef ^^end_list ];

define def_prefix_parser();
    lvars template = read_expr();
    pop11_need_nextreaditem( ":" ) -> _;
    lvars stmnts = read_stmnt_seq( true );
    pop11_need_nextreaditem( def_end_list ) -> _;
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

;;; --- switch -----------------------------------------------------------------

lconstant switch_end_list = [ switch ^^end_list ];

define switch_prefix_parser();
    dlocal popnewline = false;
    lvars selector_expr = newSingleValue( read_expr() );
    lvars cases_exprs = new_list_builder();
    lvars else_expr = false;
    until pop11_try_nextreaditem( switch_end_list ) do
        if pop11_try_nextreaditem( "else" ) then
            pop11_try_nextreaditem( ":" ) -> _;
            if else_expr then
                mishap( 'Multiple else clauses in switch expression', [] )
            endif;
            read_stmnt_seq( true ) -> else_expr
        else
            pop11_need_nextreaditem( "case" ) -> _;
            lvars c_predicate = newSingleValue( read_expr() );
            if nextreaditem() == "case" then
                cases_exprs( newCaseThen( c_predicate, FallThru ) )
            else
                lvars item = pop11_need_nextreaditem( [then :] );
                lvars c_action = read_stmnt_seq( true );
                cases_exprs( newCaseThen( c_predicate, c_action) )
            endif
        endif
    enduntil;
    newSwitch( selector_expr, cases_exprs.list_builder_newlist, else_expr )
enddefine;
switch_prefix_parser -> prefix_table( "switch" );

;;; --- $( ---------------------------------------------------------------------

procedure() with_props hole_prefix_parser;
    newHole()
endprocedure -> prefix_table( "_" );

define dollar_prefix_parser() -> expr;
    dlocal popnewline = false;
    pop11_need_nextreaditem( "(" ) -> _;
    lvars expr_with_holes = read_expr_seq();
    lvars ( expr_without_holes, new_tmp_vars ) = replace_holes( expr_with_holes );
    pop11_need_nextreaditem( ")" ) -> _;
    newFn(
        false,
        newSeq(#| applist( new_tmp_vars.rev, newId ) |#),
        expr_without_holes
    ) -> expr;
enddefine;
dollar_prefix_parser -> prefix_table( "$" );

;;; --- ( ----------------------------------------------------------------------

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
    lvars rhs = read_expr_prec( prec );
    newBind( lhs, rhs )
enddefine;
consPostfixEntry( 990, bind_postfix_parser ) -> postfix_table( ":=" );

vars current_lhs = false;
define assign_postfix_parser( prec, lhs, token );
    dlocal current_lhs = lhs;
    lvars rhs = read_expr_prec( prec );
    newAssign( lhs, rhs )  
enddefine;
consPostfixEntry( 990, assign_postfix_parser ) -> postfix_table( "<-" );

define dollardollar_prefix_parser();
    if current_lhs then
        ;;; TODO: shortcut that is unlikely to be entirely correct.
        copydata( current_lhs )
    else
        mishap( 'Left-hand-side of assignment syntax used outside of an assignment expression', [] )
    endif
enddefine;
dollardollar_prefix_parser -> prefix_table( "$$" );

;;; --- Arithmetic -------------------------------------------------------------

define infix_postfix_parser( prec, lhs, token );
    lvars rhs = read_expr_prec( prec );
    newApply( newId( token ), newSeq(#| lhs, rhs |#) )
enddefine;

consPostfixEntry( 190, infix_postfix_parser ) -> postfix_table( "+" );
consPostfixEntry( 180, infix_postfix_parser ) -> postfix_table( "-" );
consPostfixEntry( 180, infix_postfix_parser ) -> postfix_table( "*" );
consPostfixEntry( 180, infix_postfix_parser ) -> postfix_table( "/" );

consPostfixEntry( 570, infix_postfix_parser ) -> postfix_table( "<" );
consPostfixEntry( 580, infix_postfix_parser ) -> postfix_table( "==" );

;;; --- \ (Nonfix) -------------------------------------------------------------

procedure() with_props nonfix_prefix_parser;
    lvars item = readitem();
    if item.isword then
        newId( item )
    elseif item.isstring then
        newId( consword( item ) )
    else
        mishap( 'Word or string needed', [ ^item ] )
    endif
endprocedure -> prefix_table( "\" );

;;; --- pass -------------------------------------------------------------------

procedure() with_props pass_prefix_parser;
    newSeq( 0 )
endprocedure -> prefix_table( "pass" );

;;; --- for --------------------------------------------------------------------
;;; ForExpression ::= 'for' Query ('do'|':') Statements ('end'|'endfor')
;;; Query ::= Pattern 'in' expression

lconstant do_colon = [ : do ];

define in_postfix_parser( prec, lhs, item );
    if lhs.isId then
        lvars rhs = read_expr();
        newIn( lhs, rhs )
    else
        mishap( 'Only single-variable patterns supported at the moment', [ ^lhs ] )
    endif
enddefine;
consPostfixEntry( 910, in_postfix_parser ) -> postfix_table( "in" );

define read_query();
    lvars e = read_expr();
    if e.isIn then
        e
    else
        mishap( 'Loop query needed', [ ^e ] )
    endif
enddefine;

lconstant for_end_list = [ endfor ^^end_list ];

procedure() with_props for_prefix_parser;
    lvars e = read_query();
    pop11_need_nextreaditem( do_colon ) -> _;
    lvars s = read_stmnt_seq( true );
    pop11_need_nextreaditem( for_end_list ) -> _;
    newFor( e, s )
endprocedure -> prefix_table( "for" );

endsection;

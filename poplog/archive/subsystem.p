compile_mode :pop11 +strict;
section $-palantir => spice;
lblock


;;; And now for the arcane part of the system - adding a new
;;; subsystem to Poplog.  This is no better now than it ever
;;; was.  You can do it, of course, but just RTFM will get you
;;; no where.  Read the code below and be enlightened - if that's
;;; the word for it.



;;; Problem: this procedure does not know whether it is being called
;;;     during an interrupt or the normal start of compilation.  So
;;;     just in case you have to emit an initial newline!  This obviously
;;;     is superfluous in the case of normal initialisation - a defect
;;;     you can see in every Poplog compiler, incidentally, so I take this
;;;     as an admission that there is no easy solution owing to the poor
;;;     design of the sys_subsystem_table.
;;;
define spice_reset();
    appdata( '\nSpice ready\n', cucharout )
enddefine;

;;; Problem: -subsystem_add_new- does not respect procedural values for
;;;     subsystem prompts.  What a joke.  So you have to do something.
;;;     By "good" fortune, ved_im does not understand sys_subsystem_table
;;;     and so you can (indeed, must) manually correct this blunder.
;;;
subsystem_add_new(
    "spice",
    {%
        procedure() with_nargs 1 with_props spice_compiler_wrapper; spice_compile() endprocedure,
        spice_reset
    %},
    spice_extn,
    spice_prompt(),
    [],
    'Spice'
);

;;; Problem: ved_im does not seem to respond to sys_subsystem_table at
;;;     all, so you need to take action independently.  This is always
;;;     fragile because VED is always on the move.
;;;
;;; Problem: adding to vedfiletypes so that you do not have duplicates
;;;     requires attention.
;;;
[%
    [ '.spi' { popcompiler ^spice_compile } ],
    ;;; [ '.spi' { vedwriteable ^false } ],
    [ '.spi' { vedlmr_print_in_file 'output.spi' } ],
    lvars line;
    for line in vedfiletypes do
        unless line.hd = '.spi' do
            line
        endunless
    endfor;
%] -> vedfiletypes;

;;; Problem: You need to instruct VED that even though this extension
;;;     corresponds to a programming language it must be a non-break
;;;     file.
unless member( '.spi', vednonbreakfiles ) do
    ['.spi' ^^vednonbreakfiles] -> vednonbreakfiles;
endunless;

;;; Problem: There is no general way of switching between subsystems.
;;; It has become common "practice" to provide a Pop11 macro for
;;; switching from Pop11 into another named subsystem.  Yuck!

define macro spice;
    "spice" -> sys_compiler_subsystem( `c` )
enddefine;

/*
Summary: The above litany of complaints should inform the reader that
the entire concept of sys_subsystem_table is mistaken.  In these days
of object-oriented programming, we should see that a more elegant
approach would have been to subclass "class subsystem" and that
installation should be as simple as
    register_subsystem( my_subsystem )
which will index the subsystem on
    my_subsystem.subsystem_name
i.e.
    my_subsystem -> sys_subsystem( my_subsystem.subsystem_name )
I really think there is no excuse for not having a lightweight
object-oriented system built-in these days.  Having lists of
procedures of optional length whose behaviour interacts in odd
ways is daft.
*/

;;; -- Autoloader Subsystems ----------------------------------
;;;
;;; We want to be able to use ENTER l1 RETURN for well-known
;;; extensions.
;;;

define add_autoloader( extn );
    lvars ssname = "spice" <> extn;
    lvars extn_str = extn.word_string;
    lvars procedure autoloader_compiler = (
        procedure( cucharin ) with_props autoloader_as_compiler;
            dlocal cucharin;
            lvars procedure autoloader = spice_autoload_table( extn );
            lvars ( pname, facet, id ) = discover_location( cucharin );
            [ pname = ^pname; facet = ^facet; id = ^id ] =>
            unless facet and id do
                mishap( 'Could not discover facet and id', [ ^facet ^id ] )
            endunless;
            dlocal spice_current_package = fetchPackageShort( pname );
            unless spice_current_package do
                mishap( 'Current package not loaded', [ ^pname ] )
            endunless;
            autoloader( id, facet.fetch_facet_number, cucharin, extn ) -> _
        endprocedure,
    );
    subsystem_add_new(
       ssname,
       {%
            autoloader_compiler,
            procedure() with_props autoloader_resetter;
                appdata( 'Spice' sys_>< extn sys_>< ' ready\n', cucharout )
            endprocedure
        %},
        extn_str,
        'spice' sys_>< extn sys_>< ': ',
        [],
        'Spice ' sys_>< extn sys_>< ' Autoloader'
    );
    [%
        [ ^extn_str { subsystem " ^ssname } ],
        lvars line;
        for line in vedfiletypes do
            unless line.hd = extn_str do
                line
            endunless
        endfor;
    %] -> vedfiletypes;
enddefine;

lblock
    lvars extn;
    for extn in [% fast_appproperty( spice_autoload_table, erase ) %] do
        unless extn == "'.spi'" do
            add_autoloader( extn )
        endunless
    endfor;
endlblock;

endlblock
endsection;

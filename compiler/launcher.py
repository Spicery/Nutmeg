import argparse
import sys
from resolver import resolveFile
from optimizer import optimizeFile
from codegen import codeGenFile
from bundler import bundleFile, createBundleFile
from tracer import traceFile
import nutmeg_extensions
from pathlib import Path
import compiler

### WARNING: The next two imports do indeed do something useful - via decorators.
### DO NOT REMOVE (unless you _really_ know what you're doing)
import dot_txt_parser
import dot_nutmeg_parser


###############################################################################
# Classes that implement the command-line functionality for each component
###############################################################################

class Launcher:

    def __init__( self, args ):
        self._args = args

    def launch(self):
        print( f"Not yet implemented: {self._args}" )


class ParseLauncher(Launcher):

    def launch( self ):
        fname = self._args.input.name
        # Note that stdin and stdout are to be treated as Nutmeg code.
        effective_fname = '.nutmeg' if fname == '<stdin>' or fname == '<stdout>' else fname
        (match, parser) = nutmeg_extensions.findMatchingParser( effective_fname  )
        for codelet in parser( self._args.input, match ):
            codelet.serialize( self._args.output )


class ResolveLauncher( Launcher ):

    def launch( self ):
        tree = resolveFile( self._args.input )
        tree.serialize( self._args.output )


class OptimizeLauncher( Launcher ):

    def launch( self ):
        tree = optimizeFile( self._args.input )
        tree.serialize( self._args.output )


class CodegenLauncher( Launcher ):

    def launch( self ):
        tree = codeGenFile( self._args.input )
        tree.serialize( self._args.output )


class BundlerLauncher( Launcher ):

    def launch( self ):
        bundle_file = self._args.bundle
        bundleFile( bundle_file, self._args.input, self._args.entry_point )

class TracerLauncher( Launcher ):

    def launch( self ):
        bundle_file = self._args.bundle
        traceFile( bundle_file )


class CompilerLauncher(Launcher):

    def __init__( self, args ):
        super().__init__( args )
        if self._args.bundle is None:
            if self._args.files:
                self._args.bundle = Path( self._args.files[ 0 ] )
                self._args.files = self._args.files[ 1: ]
            else:
                raise Exception( 'No bundle file provided' )

    def launch( self ):
        cmplr = compiler.Compiler( self._args.entry_point, self._args.bundle, tuple( map( Path, self._args.files ) )  )
        cmplr.compile()


class RunLauncher(Launcher):
    """
    We expect this will fork-and-exec into the C# monolithic runtime-engine 
    after preparing the arguments. 
    """
    pass

    
###############################################################################
# Main entry point - parses the options and launches the right phase.
###############################################################################

def main():
    parser = argparse.ArgumentParser(
        prog="nutmeg",
        description="""
            The nutmeg command is used to run part of or all of nutmeg\'s toolchain.
            Typically is is used to compile nutmeg files together to produce a
            runnable "bundle" file or to run a bundle-file. But by specifying the mode,
            it can be used to run any single part of the toolchain.	
            """,
    )
    parser.set_defaults(mode=RunLauncher)

    subparsers = parser.add_subparsers(
        help="Selects which part(s) of the nutmeg system to use"
    )

    mode_parse = subparsers.add_parser(
        "parse", help="Parses nutmeg source code to generate a tree"
    )
    mode_parse.set_defaults( mode=ParseLauncher )
    mode_parse.add_argument( "--input", type=argparse.FileType("r"), default=sys.stdin )
    mode_parse.add_argument( "--output", type=argparse.FileType("w"), default=sys.stdout )

    mode_resolve = subparsers.add_parser(
        "resolve", help="Annotates a tree with scope information"
    )
    mode_resolve.set_defaults( mode=ResolveLauncher )
    mode_resolve.add_argument( "--input", type=argparse.FileType("r"), default=sys.stdin )
    mode_resolve.add_argument( "--output", type=argparse.FileType("w"), default=sys.stdout )

    mode_optimize = subparsers.add_parser(
        "optimize", help="Transforms a tree to improve performance"
    )
    mode_optimize.set_defaults( mode=OptimizeLauncher )
    mode_optimize.add_argument("--input", type=argparse.FileType("r"), default=sys.stdin )
    mode_optimize.add_argument("--output", type=argparse.FileType("w"), default=sys.stdout )

    mode_codegen = subparsers.add_parser(
        "codegen", help="Transforms a tree into back-end code"
    )
    mode_codegen.set_defaults( mode=CodegenLauncher )
    mode_codegen.add_argument( "--input", type=argparse.FileType("r"), default=sys.stdin )
    mode_codegen.add_argument( "--output", type=argparse.FileType("w"), default=sys.stdout )

    mode_bundler = subparsers.add_parser(
        "bundle", help="Adds trees into the bundle file"
    )
    mode_bundler.set_defaults( mode=BundlerLauncher  )
    mode_bundler.add_argument( "--input", type=argparse.FileType("r"), default=sys.stdin )
    mode_bundler.add_argument( "--bundle", type=Path, required=True )
    mode_bundler.add_argument( "--entry-point", "-e", action='append' )

    mode_tracer = subparsers.add_parser(
        "trace", help="Infers dependencies for entry-points"
    )
    mode_tracer.set_defaults( mode=TracerLauncher )
    mode_tracer.add_argument( "--bundle", type=Path, required=True )

    mode_compile = subparsers.add_parser(
        "compile", help="Compiles nutmeg files to produce a bundle-file"
    )
    mode_compile.set_defaults( mode=CompilerLauncher )
    mode_compile.add_argument( "--bundle", "-b", type=Path )
    mode_compile.add_argument( "--entry-point", "-e", action='append' )
    mode_compile.add_argument( 'files', nargs = argparse.REMAINDER )

    mode_run = subparsers.add_parser( "run", help="Runs a bundle-file" )
    mode_run.set_defaults( mode=RunLauncher )

    args = parser.parse_args()
    args.mode(args).launch()


################################################################################
# This is the top-level entry point for the whole Nutmeg system.
################################################################################

if __name__ == "__main__":
    main()

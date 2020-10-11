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
import os
import subprocess

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
                raise Exception( 'COMPILER: No bundle file provided' )

    def launch( self ):
        cmplr = compiler.Compiler( self._args.entry_point, self._args.bundle, tuple( map( Path, self._args.files ) )  )
        cmplr.compile()


class RunLauncher( Launcher ):

    def __init__( self, args ):
        super().__init__( args )
        if self._args.bundle is None:
            if self._args.others:
                self._args.bundle = Path( self._args.others[ 0 ] )
                self._args.others = self._args.others[ 1: ]
            else:
                raise Exception( 'RUN: No bundle file provided' )
        if self._args.entry_point is None:
            if self._args.others:
                self._args.entry_point = Path( self._args.others[ 0 ] )
                self._args.others = self._args.others[ 1: ]
            else:
                raise Exception( 'RUN: No entry point provided' )

    def launch( self ):
        """
        Exec into the runner monolith
        """
        executable = Path( Path( os.environ[ 'NUTMEG_HOME' ] ), Path( "runner/NutmegRunner" ) )
        command = [ executable, f"--entry-point={self._args.entry_point}", self._args.bundle ]
        subprocess.run( command )
        # try:
        #     pid = os.fork()
        # except OSError as e:
        #     raise Exception( f"{e.strerror} [{e.errno}]" )
        # if pid == 0:
        #     # Child
        #     os.execlp( "/bin/echo", executable, f"--entry-point={self._args.entry_point}", f"--bundle={self._args.bundle}" )
        # else:
        #     os._exit( 0 )

    
###############################################################################
# Main entry point - parses the options and launches the right phase.
###############################################################################

COMMANDS = {
    "parser" : "parse",
    "resolver": "resolve",
    "optimizer" : "optimize",
    "codegen" : "codegen",
    "bundler" : "bundle",
    "tracer" : "trace",
    "compiler" : "compile",
    "runner": "run"
}

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
        COMMANDS[ "parser" ], help="Parses nutmeg source code to generate a tree"
    )
    mode_parse.set_defaults( mode=ParseLauncher )
    mode_parse.add_argument( "--input", type=argparse.FileType("r"), default=sys.stdin )
    mode_parse.add_argument( "--output", type=argparse.FileType("w"), default=sys.stdout )

    mode_resolve = subparsers.add_parser(
        COMMANDS[ "resolver" ], help="Annotates a tree with scope information"
    )
    mode_resolve.set_defaults( mode=ResolveLauncher )
    mode_resolve.add_argument( "--input", type=argparse.FileType("r"), default=sys.stdin )
    mode_resolve.add_argument( "--output", type=argparse.FileType("w"), default=sys.stdout )

    mode_optimize = subparsers.add_parser(
        COMMANDS[ "optimizer" ], help="Transforms a tree to improve performance"
    )
    mode_optimize.set_defaults( mode=OptimizeLauncher )
    mode_optimize.add_argument("--input", type=argparse.FileType("r"), default=sys.stdin )
    mode_optimize.add_argument("--output", type=argparse.FileType("w"), default=sys.stdout )

    mode_codegen = subparsers.add_parser(
        COMMANDS[ "codegen" ], help="Transforms a tree into back-end code"
    )
    mode_codegen.set_defaults( mode=CodegenLauncher )
    mode_codegen.add_argument( "--input", type=argparse.FileType("r"), default=sys.stdin )
    mode_codegen.add_argument( "--output", type=argparse.FileType("w"), default=sys.stdout )

    mode_bundler = subparsers.add_parser(
        COMMANDS[ "bundler" ], help="Adds trees into the bundle file"
    )
    mode_bundler.set_defaults( mode=BundlerLauncher  )
    mode_bundler.add_argument( "--input", type=argparse.FileType("r"), default=sys.stdin )
    mode_bundler.add_argument( "--bundle", type=Path, required=True )
    mode_bundler.add_argument( "--entry-point", "-e", action='append' )

    mode_tracer = subparsers.add_parser(
        COMMANDS[ "tracer" ], help="Infers dependencies for entry-points"
    )
    mode_tracer.set_defaults( mode=TracerLauncher )
    mode_tracer.add_argument( "--bundle", type=Path, required=True )

    mode_compile = subparsers.add_parser(
        COMMANDS[ "compiler" ], help="Compiles nutmeg files to produce a bundle-file"
    )
    mode_compile.set_defaults( mode=CompilerLauncher )
    mode_compile.add_argument( "--bundle", "-b", type=Path )
    mode_compile.add_argument( "--entry-point", "-e", action='append' )
    mode_compile.add_argument( 'files', nargs = argparse.REMAINDER )

    mode_run = subparsers.add_parser( COMMANDS[ "runner" ], help="Runs a bundle-file" )
    mode_run.set_defaults( mode=RunLauncher )
    mode_run.add_argument( "--bundle", "-b", type=Path )
    mode_run.add_argument( "--entry-point", "-e", type=str )
    mode_run.add_argument( 'others', nargs = argparse.REMAINDER )

    # Handle the case when the subparser command is omitted.
    argv = sys.argv[1:]

    if len( argv ) == 0 or argv[0] not in COMMANDS.values():
        argv = [ COMMANDS[ "runner" ], *argv ]

    args = parser.parse_args( args=argv )
    args.mode(args).launch()


################################################################################
# This is the top-level entry point for the whole Nutmeg system.
################################################################################

if __name__ == "__main__":
    main()

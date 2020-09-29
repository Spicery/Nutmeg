import argparse
import sys
import codetree
from resolver import Resolver
from parser import parseFromFileObject

import nutmeg_extensions

### WARNING: The next two imports do indeed do something useful - via decorators.
import dot_txt_parser
import dot_nutmeg_parser

###############################################################################
# Components
###############################################################################

# There will be a class for each component - and they will all be moved
# to their own modules. 


###############################################################################
# Classes that implement the command-line functionality for each component
###############################################################################

class Launcher:

    def __init__(self, args):
        self._args = args

    def launch(self):
        print(f"Not yet implemented: {self._args}")


class ParseLauncher(Launcher):

    def launch(self):
        fname = self._args.input.name
        # Note that stdin and stdout are to be treated as Nutmeg code.
        effective_fname = '.nutmeg' if fname == '<stdin>' or fname == '<stdout>' else fname
        (match, parser) = nutmeg_extensions.findMatchingParser( effective_fname  )
        for codelet in parser( self._args.input, match ):
            codelet.serialise( self._args.output )


class ResolveLauncher( Launcher ):
    def launch( self ):
        tree = Resolver().resolveFile( self._args.input )
        tree.serialise( self._args.output )

class OptimiseLauncher(Launcher):
    """Placeholder"""
    pass


class GenCodeLauncher(Launcher):
    """Placeholder"""
    pass


class CompileLauncher(Launcher):
    """Placeholder"""
    pass


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
    mode_parse.set_defaults(mode=ParseLauncher)
    mode_parse.add_argument("--input", type=argparse.FileType("r"), default=sys.stdin)
    mode_parse.add_argument("--output", type=argparse.FileType("w"), default=sys.stdout)

    mode_resolve = subparsers.add_parser(
        "resolve", help="Annotates a tree with scope information"
    )
    mode_resolve.set_defaults(mode=ResolveLauncher)
    mode_resolve.add_argument("--input", type=argparse.FileType("r"), default=sys.stdin)
    mode_resolve.add_argument("--output", type=argparse.FileType("w"), default=sys.stdout)

    mode_optimise = subparsers.add_parser(
        "optimise", help="Transforms a tree to improve performance"
    )
    mode_optimise.set_defaults(mode=OptimiseLauncher)

    mode_gencode = subparsers.add_parser(
        "gencode", help="Transforms a tree into back-end code"
    )
    mode_gencode.set_defaults(mode=GenCodeLauncher)

    mode_compile = subparsers.add_parser(
        "compile", help="Compiles nutmeg files to produce a bundle-file"
    )
    mode_compile.set_defaults(mode=CompileLauncher)

    mode_run = subparsers.add_parser("run", help="Runs a bundle-file")
    mode_run.set_defaults(mode=RunLauncher)

    args = parser.parse_args()
    args.mode(args).launch()


################################################################################
# This is the top-level entry point for the whole Nutmeg system.
################################################################################

if __name__ == "__main__":
    main()

#!/usr/bin/python3

import argparse
import tempfile
from pathlib import Path
import subprocess

def main( args ):
    tmpdir = Path( tempfile.mkdtemp() )
    bundle_file = tmpdir / 'tmp.bundle'
    if args.verbose: print( 'Bundle file will be:', bundle_file )
    try:
        for folder in args.folders:
            if args.verbose: print( 'Running test for:', folder )
            nutmeg_files = list( Path( folder ).glob( '*.nutmeg' ) )
            if args.verbose: print( 'Compiling files:', nutmeg_files )
            if bundle_file.exists():
                bundle_file.unlink()
            subprocess.run( [ 'nutmegc', bundle_file ] + nutmeg_files )
            if args.verbose: print( ' - Compiled' )
            if args.verbose: print( 'Testing' )
            subprocess.run( [ 'nutmeg', 'unittest', f'--title={folder}', bundle_file ] )
            if args.verbose: print( ' - Tested' )
    finally:
        if args.verbose: print( 'Cleaning up temporary folder:', tmpdir )
        if bundle_file.exists():
            bundle_file.unlink()    # missing_ok=True allowed from Python 3.8 onwards
        tmpdir.rmdir()
        if args.verbose: print( ' - Cleaned' )


if __name__ == "__main__":
    parser = argparse.ArgumentParser( prog='UnitTest_Here' )
    parser.add_argument( '--verbose', action='store_true' )
    parser.add_argument( 'folders', nargs='*')
    args = parser.parse_args()
    if args.verbose: print( args )
    main( args )
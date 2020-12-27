#!/usr/bin/python3

import os
import argparse

TEMPLATE = """#!/bin/bash
case $1 in
    compile|parse|resolve|optimize|codegen|bundle|trace)
        exec $(INSTALL_DIR)/compiler/nutmeg $*
        ;;
    script)
        shift
        tfile=$(mktemp -u)
        $(INSTALL_DIR)/compiler/nutmeg compile --bundle="$tfile" $* && $(INSTALL_DIR)/runner/NutmegRunner "$tfile"
        /bin/rm -f "$tfile"
        ;;
    unittest)
        shift
        exec $(INSTALL_DIR)/runner/NutmegRunner --unittest $*
        ;;
    run)
        shift
        exec $(INSTALL_DIR)/runner/NutmegRunner $*
        ;;
    *)
        exec $(INSTALL_DIR)/runner/NutmegRunner $*
        ;;
esac
"""

if __name__ == "__main__":
    parser = argparse.ArgumentParser( prog='mkinstaller' )
    parser.add_argument( "--install_dir", required=True )
    args = parser.parse_args()
    print( TEMPLATE.replace( '$(INSTALL_DIR)', args.install_dir ) )

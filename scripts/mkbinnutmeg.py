#!/usr/bin/python3

import os
import argparse

TEMPLATE = """#!/bin/bash
case $1 in
    run|unittest|script|info)
        case $2 in
            --help|-h)
                exec {{{INSTALL_DIR}}}/compiler/nutmeg $1 --help
                ;;
        esac
        ;;
    compile|parse|resolve|optimize|codegen|bundle|trace|help)
        exec {{{INSTALL_DIR}}}/compiler/nutmeg $*
        ;;
    --help)
        exec {{{INSTALL_DIR}}}/compiler/nutmeg --help
        ;;
    *)
        set - run "$*"
        ;;
esac
case $1 in
    run)
        shift
        exec {{{INSTALL_DIR}}}/runner/NutmegRunner $*
        ;;
    script)
        shift
        tfile=$(mktemp -u)
        {{{INSTALL_DIR}}}/compiler/nutmeg compile --bundle="$tfile" $* && {{{INSTALL_DIR}}}/runner/NutmegRunner "$tfile"
        exec /bin/rm -f "$tfile"
        ;;
    unittest)
        shift
        exec {{{INSTALL_DIR}}}/runner/NutmegRunner --unittest $*
        ;;
    info)
        shift
        exec {{{INSTALL_DIR}}}/runner/NutmegRunner --info $*
        ;;
    *)
        echo "Error in nutmeg script" >&2
        exit 1
        ;;
esac
"""

if __name__ == "__main__":
    parser = argparse.ArgumentParser( prog='mkinstaller' )
    parser.add_argument( "--install_dir", required=True )
    args = parser.parse_args()
    script = TEMPLATE.replace( '{{{INSTALL_DIR}}}', args.install_dir )
    print( script )

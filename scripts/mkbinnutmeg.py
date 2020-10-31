#!/usr/bin/python3

import os
import argparse

TEMPLATE = """
#!/bin/bash
case $1 in
    compile|parse|resolve|optimize|codegen|bundle|trace)
        exec /opt/nutmeg/libexec/nutmeg/compiler/nutmeg $*
        ;;
    test)
        shift
        exec $(INSTALL_DIR)/runner/NutmegRunner --test $*
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

#!/usr/bin/python3

import os
import argparse

TEMPLATE = """#!/bin/bash
/bin/rm -rf $(INSTALL_DIR)-old
if [ -d "$(INSTALL_DIR)" ]; mv $(INSTALL_DIR) $(INSTALL_DIR)-old; fi
( /usr/bin/tar cf - libexec/nutmeg ) | ( cd $(INSTALL_DIR); /usr/bin/tar xf - )
cp bin/nutmeg $(EXEC_DIR)
cp bin/nutmegc $(EXEC_DIR)"""

if __name__ == "__main__":
    parser = argparse.ArgumentParser( prog='mkinstaller' )
    parser.add_argument( "--install_dir", required=True )
    parser.add_argument( "--exec_dir", required=True )
    args = parser.parse_args()
    print( TEMPLATE.replace( '$(INSTALL_DIR)', args.install_dir ).replace( '$(EXEC_DIR)', args.exec_dir ) )

#!/usr/bin/python3

import os
import argparse

TEMPLATE = """#!/bin/bash
exec {{{INSTALL_DIR}}}/compiler/nutmeg compile $*
"""

if __name__ == "__main__":
    parser = argparse.ArgumentParser( prog='mkinstaller' )
    parser.add_argument( "--install_dir", required=True )
    args = parser.parse_args()
    script = TEMPLATE.replace( '{{{INSTALL_DIR}}}', args.install_dir )
    print( script )

from pathlib import Path
import sqlite3
import io

import nutmeg_extensions
import dot_nutmeg_parser
import dot_txt_parser

from bundler import createBundleFile, clearBundleFile
import bundler
import resolver
import optimizer
import codegen
import tracer

class Compiler:

    def __init__( self, entry_points: [str], bundle : Path, files : [Path], keep=False ):
        self._entry_points = entry_points or []
        self._bundle = bundle
        self._files = files
        self._keep = keep

    def getAndAddSource( self, file : Path ):
        with sqlite3.connect( self._bundle ) as conn:
            c = conn.cursor()
            text = file.read_text()
            c.execute( '''INSERT OR REPLACE INTO SourceFiles VALUES ( ?, ? )''', ( str(file.absolute()), text ) )
            return text

    def processOneFile( self, filename ):
        text = self.getAndAddSource( filename )
        fname = filename.name
        (match, parser) = nutmeg_extensions.findMatchingParser( fname  )
        for codelet in parser( io.StringIO( text ), match ):
            resolver.resolveCodeTree( codelet )                                 # Edits in place.
            codelet = optimizer.optimizeCodeTree( codelet )
            codegen.codeGenCodeTree( codelet )                                  # Also edits in place
            bundler.bundleCodeTree( self._bundle, codelet )

    def compile( self ):
        if not self._bundle.exists():
            createBundleFile( self._bundle )
        elif not self._keep:
            clearBundleFile( self._bundle )
        for filename in self._files:
            if filename.exists():
                self.processOneFile( filename )
            else:
                raise Exception( f'Cannot find file to open: {filename}' )
        bundler.addEntryPoints( self._bundle, self._entry_points )
        tracer.traceFile( self._bundle )
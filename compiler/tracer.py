"""
Traces dependencies
"""

import sqlite3
import codetree
from codetree import IdCodelet

class Scanner:

    def __init__( self ):
        self._found = set()

    def found( self ):
        result = self._found
        self._found = {}
        return result

    def scan( self, expr ):
        if isinstance( expr, IdCodelet ) and expr.scope() == "global":
            self._found.add( expr.name() )
        else:
            for c in expr.members():
                self.scan( c )

def shallowFindDependencies( expr ):
    s = Scanner()
    s.scan( expr )
    return s.found()

def deepFindDependencies( cursor, name ):
    open_set = set( ( name, ) )
    closed_set = set()
    while open_set:
        gname = open_set.pop()
        valueAsStr = cursor.execute( '''SELECT Value FROM Bindings WHERE IdName = ?''', (gname,) ).fetchone()
        if valueAsStr:
            value = codetree.codeTreeFromJSONString( valueAsStr[ 0 ] )
            dependencies = shallowFindDependencies( value )
            closed_set.add( gname )
            open_set.update( d for d in dependencies if d not in closed_set )
        else:
            raise Exception( f'Global variable "{gname}" is referenced but not defined' )
    return closed_set

def traceFile( bundle_file ):
    with sqlite3.connect( bundle_file ) as conn:
        c = conn.cursor()
        c.execute( '''BEGIN TRANSACTION''' )
        c.execute( '''DELETE FROM DependsOn''' )
        entry_points = tuple( row[0] for row in c.execute( '''SELECT IdName FROM EntryPoints''' ) )
        for e in entry_points:
            for d in deepFindDependencies( c, e ):
                c.execute( '''INSERT INTO DependsOn VALUES ( ?, ? )''', ( e, d ) )
        c.execute( '''COMMIT TRANSACTION''' )




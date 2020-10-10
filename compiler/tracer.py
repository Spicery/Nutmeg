"""
Traces dependencies
"""

import sqlite3
import codetree
import syscalls
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
    sysfns_set = set()
    while open_set:
        gname = open_set.pop()
        valueAsStr = cursor.execute( '''SELECT Value FROM Bindings WHERE IdName = ?''', (gname,) ).fetchone()
        if valueAsStr:
            value = codetree.codeTreeFromJSONString( valueAsStr[ 0 ] )
            dependencies = shallowFindDependencies( value )
            closed_set.add( gname )
            open_set.update( d for d in dependencies if d not in closed_set )
        elif syscalls.isSysconst( gname ):
            sysfns_set.add( gname )
        else:
            raise Exception( f'Global variable "{gname}" is referenced but not defined' )
    return closed_set, sysfns_set

def traceFile( bundle_file ):
    with sqlite3.connect( bundle_file ) as conn:
        c = conn.cursor()
        c.execute( '''BEGIN TRANSACTION''' )
        c.execute( '''DELETE FROM DependsOn''' )
        entry_points = tuple( row[0] for row in c.execute( '''SELECT IdName FROM EntryPoints''' ) )
        all_sysfns = set()
        for e in entry_points:
            dependencies, sysconsts = deepFindDependencies( c, e )
            all_sysfns.update( sysconsts )
            for d in dependencies:
                c.execute( '''INSERT INTO DependsOn VALUES ( ?, ? )''', ( e, d ) )
            for d in sysconsts:
                c.execute( '''INSERT INTO DependsOn VALUES ( ?, ? )''', (e, d) )
        for sysfnname in all_sysfns:
            c.execute( '''INSERT INTO Bindings VALUES ( ?, ? )''', ( sysfnname, f'{{"kind":"sysfn","name":"{sysfnname}"}}' ) )
        c.execute( '''COMMIT TRANSACTION''' )




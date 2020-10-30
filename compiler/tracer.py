"""
Traces dependencies
"""

import sqlite3
import codetree
import syscalls
from codetree import IdCodelet
from mishap import Mishap

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

def doShallowFindDependencies( expr ):
    s = Scanner()
    s.scan( expr )
    return s.found()

class DeepFinder:

    def __init__( self, cursor ):
        self._cursor = cursor
        self._deepMemo = {}
        self._shallowMemo = {}

    def deepFindDependencies( self, name ):
        if name in self._deepMemo:
            return self._deepMemo[ name ]
        else:
            closed_set, sysfns_set = self.doDeepFindDependencies( name )
            self._deepMemo[ name ] = ( closed_set, sysfns_set )
            return closed_set, sysfns_set

    def shallowFindDependencies( self, name, value ):
        if name in self._shallowMemo:
            return self._shallowMemo[ name ]
        else:
            found = doShallowFindDependencies( value )
            self._shallowMemo[ name ] = found
            return found

    def doDeepFindDependencies( self, name ):
        open_set = set( ( name, ) )
        closed_set = set()
        sysfns_set = set()
        while open_set:
            gname = open_set.pop()
            valueAsStr = self._cursor.execute( '''SELECT Value FROM Bindings WHERE IdName = ?''', (gname,) ).fetchone()
            if valueAsStr:
                value = codetree.codeTreeFromJSONString( valueAsStr[ 0 ] )
                dependencies = self.shallowFindDependencies( name, value )
                closed_set.add( gname )
                open_set.update( d for d in dependencies if d not in closed_set )
            elif syscalls.isSysconst( gname ):
                sysfns_set.add( gname )
            else:
                raise Mishap( f'Global variable is referenced but not defined', name=gname )
        return closed_set, sysfns_set

def traceFile( bundle_file ):
    with sqlite3.connect( bundle_file ) as conn:
        c = conn.cursor()
        c.execute( '''BEGIN TRANSACTION''' )
        c.execute( '''DELETE FROM DependsOn''' )
        entry_points = set( row[0] for row in c.execute( '''SELECT IdName FROM EntryPoints''' ) )
        entry_points |= set( row[ 0 ] for row in c.execute( """SELECT IdName FROM Annotations WHERE AnnotationKey='unittest'""" ) )
        all_sysfns = set()
        deep_finder = DeepFinder( c )
        for e in entry_points:
            dependencies, sysconsts = deep_finder.deepFindDependencies( e )
            all_sysfns.update( sysconsts )
            for d in dependencies:
                c.execute( '''INSERT INTO DependsOn VALUES ( ?, ? )''', ( e, d ) )
            for d in sysconsts:
                c.execute( '''INSERT INTO DependsOn VALUES ( ?, ? )''', (e, d) )

        for sysfnname in all_sysfns:
            c.execute( '''INSERT INTO Bindings VALUES ( ?, ?, NULL )''', ( sysfnname, f'{{"kind":"sysfn","name":"{sysfnname}"}}' ) )
        c.execute( '''COMMIT TRANSACTION''' )

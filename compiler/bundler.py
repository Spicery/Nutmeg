import codetree
from codetree import Codelet, BindingCodelet, IdCodelet
import sqlite3
from pathlib import Path
import json

def createBundleFile( bundle_file ):
    with sqlite3.connect( bundle_file ) as conn:
        c = conn.cursor()
        c.execute( '''CREATE TABLE "Bindings" ( "IdName" TEXT, "Value" TEXT, "FileName" TEXT, PRIMARY KEY("IdName") )''' )
        c.execute( '''CREATE TABLE "DependsOn" ( "IdName" TEXT NOT NULL, "Needs" TEXT NOT NULL, PRIMARY KEY("IdName","Needs") )''' )
        c.execute( '''CREATE TABLE "SourceFiles" ( "FileName" TEXT NOT NULL, "Contents" TEXT NOT NULL, PRIMARY KEY("FileName") )''' )
        c.execute( '''CREATE TABLE "Annotations" ( "IdName" TEXT, "AnnotationKey" TEXT NOT NULL, "AnnotationValue" TEXT NOT NULL, PRIMARY KEY("IdName", "AnnotationKey") )''' )

def clearBundleFile( bundle_file ):
    with sqlite3.connect( bundle_file ) as conn:
        c = conn.cursor()
        c.execute( '''DELETE FROM "Bindings"''' )
        c.execute( '''DELETE FROM "DependsOn"''' )
        c.execute( '''DELETE FROM "SourceFiles"''' )
        c.execute( '''DELETE FROM "Annotations"''')

def bundleCodeTree( bundle_file : Path, tree : Codelet, input_file: Path ):
    if not isinstance( tree, BindingCodelet ):
        raise Exception( 'At this point in time we can only bundle bindings: {tree}' )
    lhs = tree.lhs()
    rhs = tree.rhs()
    annotations = tree.annotations()
    if not isinstance( lhs, IdCodelet ):
        raise Exception( 'At this point in time we can only bundle bindings to IdCodelets: {tree}' )
    name = lhs.name()
    value = rhs.serializeToString()
    if not bundle_file.exists():
        createBundleFile( bundle_file )
    with sqlite3.connect( bundle_file ) as conn:
        c = conn.cursor()
        c.execute( '''INSERT OR REPLACE INTO Bindings VALUES ( ?, ?, ? )''', ( name, value, str(input_file.absolute()) ) )
        for annkey, annvalue in annotations.items():
            c.execute( '''INSERT OR REPLACE INTO Annotations VALUES ( ?, ?, ? )''', ( name, annkey, json.dumps( annvalue ) ) )

def addEntryPoints( bundle_file, entry_points ):
    with sqlite3.connect( bundle_file ) as conn:
        c = conn.cursor()
        for e in entry_points:
            c.execute( """INSERT OR IGNORE INTO Annotations VALUES ( ?, 'command', 'true' )""", ( e, ) )

def bundleFile( bundle_file, input_file, entry_points ):
    tree = codetree.codeTreeFromJSONFileObject( input_file )
    bundleCodeTree( bundle_file, tree, input_file )
    addEntryPoints( bundle_file, entry_points )

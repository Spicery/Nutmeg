import codetree
from codetree import IdCodelet, SeqCodelet

def countNargs( tree ):
    if isinstance( tree, IdCodelet ):
        return 1
    elif isinstance( tree, SeqCodelet ):
        return sum( map( countNargs, tree.body() ) )
    else:
        raise Exception( f'Unexpected parameter to function: {tree}' )

class CodeGenSetSlots( codetree.CodeletVisitor ):

    def __init__( self, sofar = 0 ):
        self._sofar = sofar
        self._allocated = {}

    def visitCodelet( self, codelet ):
        for c in codelet.members():
            c.visit( self )

    def visitIdCodelet( self, id_codelet ):
        if id_codelet.scope() == "local":
            name = id_codelet.label()
            if name not in self._allocated:
                self._allocated[ name ] = self._sofar
                self._sofar += 1
            id_codelet.setSlot( self._allocated[ name ] )

    def visitFunctionCodelet( self, fn_codelet ):
        setslots = CodeGenSetSlots()
        fn_codelet.parameters().visit( setslots )
        fn_codelet.body().visit( setslots )
        nlocals = setslots._sofar
        nargs = countNargs( fn_codelet.parameters() )
        fn_codelet.setNlocals( nlocals )
        fn_codelet.setNargs( nargs )

def codeGenCodeTree( tree ):
    tree.visit( CodeGenSetSlots() )
    return tree

def codeGenFile( file ):
    tree = codetree.codeTreeFromJSONFileObject( file )
    return codeGenCodeTree( tree )


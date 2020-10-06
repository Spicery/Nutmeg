import codetree
from codetree import CallCodelet, SyscallCodelet, IdCodelet
import syscalls



class ReplaceIdsWithSysconsts( codetree.CodeletVisitor ):

    def __call__( self, tree ):
        return tree.visit( self )

    def visitCodelet( self, codelet ):
        return codelet.transform( self )

    def visitIdCodelet( self, id_codelet ):
        if syscalls.isSysconst( id_codelet.name() ):
            raise Exception( f"sysconsts not implemented yet: {id_codelet}" )
        else:
            return id_codelet

    def visitCallCodelet( self, call_codelet : CallCodelet ):
        f = call_codelet.function()
        if isinstance( f, IdCodelet ) and syscalls.isSysconst( f.name() ) and f.scope() == "global":
            # TODO: do we need to be concerned about call_codelet._kwargs?
            return SyscallCodelet( name=f.name(), arguments=call_codelet.arguments() )
        else:
            return call_codelet

def replaceIdsWithSysconsts( tree ):
    return ReplaceIdsWithSysconsts()( tree )

def optimizeCodeTree( tree ):
    tree = replaceIdsWithSysconsts( tree )
    return tree

def optimizeFile( file ):
    tree = codetree.codeTreeFromJSONFileObject( file )
    return optimizeCodeTree( tree )

    
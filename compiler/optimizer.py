import codetree
from codetree import CallCodelet, SyscallCodelet, IdCodelet
import syscalls
from resolver import newLabel


class LambdaLift( codetree.CodeletVisitor ):

    def __call__( self, tree ):
        return tree.visit( self )

    def visitCodelet( self, codelet ):
        return codelet.transform( self )

    def visitFunctionCodelet( self, fn_codelet : codetree.LambdaCodelet ):
        captured = list( fn_codelet.captured() )
        fn_codelet = codetree.LambdaCodelet( parameters=fn_codelet.parameters(), body=fn_codelet.body().visit( self ) )
        if captured:
            extra_params = [ id_codelet.copy( label=newLabel() ) for id_codelet in captured ]
            new_params = codetree.SeqCodelet(
                fn_codelet.parameters(),
                *extra_params
            )
            fn_codelet = codetree.LambdaCodelet( parameters=new_params, body=fn_codelet.body() )
            return codetree.SyscallCodelet(
                name="partApply",
                arguments=codetree.SeqCodelet( fn_codelet, *captured )
            )
        else:
            return fn_codelet


class ReplaceIdsWithSysconsts( codetree.CodeletVisitor ):

    def __call__( self, tree ):
        return tree.visit( self )

    def visitCodelet( self, codelet ):
        return codelet.transform( self )

    def visitIdCodelet( self, id_codelet ):
        if syscalls.isSysconst( id_codelet.name() ):
            raise Exception( f"sysconsts not implemented yet: {id_codelet.name()}" )
        else:
            return id_codelet

    def visitCallCodelet( self, call_codelet : CallCodelet ):
        f = call_codelet.function()
        if isinstance( f, IdCodelet ) and syscalls.isSysconst( f.name() ) and f.scope() == "global":
            # TODO: do we need to be concerned about call_codelet._kwargs?
            return SyscallCodelet( name=f.name(), arguments=call_codelet.arguments().visit( self ) )
        else:
            return call_codelet.transform( self )

class Simplify( codetree.CodeletVisitor ):

    def __call__( self, tree ):
        return tree.visit( self )

    def visitCodelet( self, codelet ):
        return codelet.transform( self )

    def visitSeqCodelet( self, code_let ):
        if len( code_let.body() ) == 1:
            return code_let.body()[ 0 ].visit( self )
        else:
            body = []
            for i in code_let.body():
                t = i.visit( self )
                if isinstance( t, codetree.SeqCodelet ):
                    body.extend( t.body() )
                else:
                    body.append( t )
            if len( body ) == 1:
                return body[ 0 ]
            else:
                return codetree.SeqCodelet( body=body )

    def visitIfCodelet( self, if_codelet ):
        test_part = if_codelet.testPart()
        if isinstance( test_part, codetree.BoolCodelet ):
            if test_part.valueAsBool():
                return if_codelet.thenPart().visit( self )
            else:
                return if_codelet.elsePart().visit( self )
        elif isinstance( test_part, codetree.SyscallCodelet ) and test_part.name() == "not":
            simplified_if = codetree.IfCodelet( testPart=test_part.arguments(), thenPart=if_codelet.elsePart(), elsePart=if_codelet.thenPart() )
            return simplified_if.visit( self )
        else:
            return self.visitCodelet( if_codelet )

    def visitSyscallCodelet( self, syscall_codelet ):
        name = syscall_codelet.name()
        args = syscall_codelet.arguments()
        if name == "newImmutableList" and isinstance( args, codetree.SyscallCodelet ):
            args_name = args.name()
            if args_name == "..<" or args_name == "...":
                transformed_args = args.arguments().visit( self )
                return codetree.SyscallCodelet( name=f'[x{args_name}y]', arguments=transformed_args )
            else:
                return self.visitCodelet( syscall_codelet )
        else:
            return self.visitCodelet( syscall_codelet )

import arity
class ArityAnalysis( codetree.CodeletVisitor ):

    def __call__( self, tree ):
        tree.visit( self )

    def visitCodelet( self, codelet ):
        for c in codelet.members():
            c.visit( self )

    def visitIdCodelet( self, code_let, *args, **kwargs ):
        code_let.setArity(1)

    def visitConstantCodelet( self, code_let, *args, **kwargs ):
        code_let.setArity( 1 )

    def visitFunctionCodelet( self, codelet, *args, **kwargs ):
        codelet.setArity( 1 )
        for c in codelet.members():
            c.visit( self )

    def visitSeqCodelet( self, codelet : codetree.SeqCodelet, *args, **kwargs ):
        sofar = arity.Arity(0)
        for c in codelet.members():
            c.visit( self )
            sofar = sofar.sum( c.arity() )
        codelet.setArity( sofar )

def lambdaLift( tree ):
    return LambdaLift()( tree )

def replaceIdsWithSysconsts( tree ):
    return ReplaceIdsWithSysconsts()( tree )

def simplifyCodeTree( tree ):
    return Simplify()( tree )

def arityAnalysisCodeTree( tree ):
    return ArityAnalysis()( tree )

def optimizeCodeTree( tree ):
    tree = lambdaLift( tree )
    tree = replaceIdsWithSysconsts( tree )
    tree = simplifyCodeTree( tree )
    arityAnalysisCodeTree( tree )
    return tree

def optimizeFile( file ):
    tree = codetree.codeTreeFromJSONFileObject( file )
    return optimizeCodeTree( tree )

    
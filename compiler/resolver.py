import codetree
import abc
from syscalls import isSysconst
from mishap import Mishap

class Scope( abc.ABC ):

    @abc.abstractmethod
    def lookup( self, id_codelet ):
        pass

    @abc.abstractmethod
    def addInfo( self, code_let ):
        pass

    @abc.abstractmethod
    def declare( self, id_let ):
        pass


class GlobalScope( Scope ):

    def lookup( self, id_codelet ):
        """Returns the scope that the name appears in, by definition this is the outermost scope"""
        return self, 0

    def addInfo( self, id_codelet ):
        id_codelet.setAsGlobal()

    def declare( self, id_codelet ):
        vname = id_codelet.name()
        if isSysconst( vname ):
            raise Mishap( "Variable name of global clashes with built-in procedure", variable=vname, hint="Built-ins cannot be redeclared, please rename your variable" )
        id_codelet.setAsGlobal()

    def hasLambda( self ):
        return False

LABEL = 0
def newLabel():
    global LABEL
    LABEL = LABEL + 1
    return LABEL

class LexicalScope( Scope ):

    def __init__( self, is_block = False, previous = None, lambdaCodelet=None ):
        self._previous = previous
        self._is_block = is_block
        self._locals = {}
        self._is_lambda = 1 if lambdaCodelet else 0
        self._lambda = lambdaCodelet if previous and previous.hasLambda() else None

    def hasLambda( self ):
        return self._is_lambda or self._previous.hasLambda()

    def lookup( self, id_codelet ):
        """Returns the scope that the name appears in"""
        name = id_codelet.name()
        if name in self._locals:
            return self, 0
        else:
            scope, lambda_nesting = self._previous.lookup( id_codelet )
            if self._lambda:
                self._lambda.capture( id_codelet )
            return scope, lambda_nesting + self._is_lambda

    def addInfo( self, code_let ):
        code_let.setAsLocal( **self._locals[ code_let.name() ] )

    def declare( self, id_codelet ):
        nm = id_codelet.name()
        if isSysconst(nm):
            raise Mishap( "Local variable trying to shadow built-in procedure", variable=nm, hint="Built-ins cannot be shadowed, please rename your variable" )
        if nm in self._locals:
            raise Exception( f'Trying to re-declare the same variable: {nm}' )
        label = newLabel()
        reftype = id_codelet.reftype()
        nonassignable = reftype == "val" or reftype == "const"
        const = reftype == "const"
        info = dict( label = label, nonassignable = nonassignable, const = const )
        self._locals[ nm ] = info
        id_codelet.declareAsLocal( **info )

class Resolver( codetree.CodeletVisitor ):
    """
    Important note: the Resolver pass is an in-place update rather than
    a copy-transform. This is fairly typical for algorithms that simply
    annotate a tree without changing the structure.

    The resolver has to find every variable in every expression and update it.
    To do this right it needs to track every scope correctly. Here's the
    list of codelets we have implemented so far:
        [x] constants, no action required
        [x] id, action depends on reftype (get/set/var/val/const/new)
        [x] sequences, just iterate over the members
        [x] syscall, just iterate over the arguments
        [x] if, iterate over the test/then/else parts
        [x] binding, iterate over the lhs & rhs
        [x] function
    """

    def visitCodelet( self, codelet, scopes ):
        """
        By default we leave the tree alone and perform no updates.
        """
        pass

    def visitIdCodelet( self, id_codelet, scopes ):
        """
        Fix up variables e.g. x
        """
        reftype = id_codelet.reftype()
        if reftype == "get" or reftype == "set":
            nm = id_codelet.name()
            scope, lambda_nesting = scopes.lookup( id_codelet )
            scope.addInfo( id_codelet )
            if lambda_nesting > 0 and not id_codelet.nonassignable():
                raise Exception( f"Cannot access assignable variable across lambda boundary: {id_codelet.name()}" )
        elif reftype == "var" or reftype == "val" or reftype == "const" or reftype == "new":
            scopes.declare( id_codelet )
        else:
            raise Exception( f'Unexpected reftype (Internal error?): {reftype}')
        if id_codelet.reftype() == "set" and id_codelet.nonassignable():
            raise Mishap( f'Trying to assign to protected variable', name=id_codelet.name() )

    def visitSeqCodelet( self, codelet, scopes ):
        for c in codelet.members():
            c.visit( self, scopes )

    def visitSyscallCodelet( self, codelet, scopes ):
        for c in codelet.members():
            c.visit( self, scopes )

    def visitSysupdateCodelet( self, codelet, scopes ):
        for c in codelet.members():
            c.visit( self, scopes )

    def visitForCodelet( self, codelet, scopes ):
        new_scope = LexicalScope( previous = scopes )
        codelet.query().visit( self, new_scope )

    def visitDoCodelet( self, codelet, scopes ):
        codelet.query().visit( self, scopes )
        codelet.body().visit( self, scopes )

    def visitWUntilCodelet( self, codelet, scopes ):
        codelet.query().visit( self, scopes )
        codelet.test().visit( self, scopes )
        codelet.result().visit( self, scopes )

    def visitAfterwardsCodelet( self, codelet, scopes ):
        codelet.query().visit( self, scopes )
        codelet.result().visit( self, scopes )

    def visitInCodelet( self, in_codelet, scopes ):
        in_codelet.pattern().visit( self, scopes )
        in_codelet.streamable().visit( self, scopes )

    def visitCallCodelet( self, codelet, scopes ):
        for c in codelet.members():
            c.visit( self, scopes )

    def visitUpdateCodelet( self, codelet, scopes ):
        for c in codelet.members():
            c.visit( self, scopes )

    def visitBindingCodelet( self, binding_codelet, scopes ):
        """
        Fix up bindings e.g.  x := EXPR
        """
        bound_id = binding_codelet.lhs()
        try:
            bound_id.visit( self, scopes )
            binding_codelet.rhs().visit( self, scopes )
        except Mishap as m:
            # Decorate the exception with additional context.
            if isinstance(bound_id, codetree.IdCodelet):
                m.addDetails( inside=bound_id.name() )
            raise m

    def visitAssignCodelet( self, assign_codelet, scopes ):
        assign_codelet.lhs().visit( self, scopes )
        assign_codelet.rhs().visit( self, scopes )

    def visitIfCodelet( self, if_codelet, scopes ):
        """
        Fix up if-expression e.g. if t then x else y endif
        """
        if_codelet.testPart().visit( self, scopes )
        if_codelet.thenPart().visit( self, LexicalScope( previous = scopes ) )
        if_codelet.elsePart().visit( self, LexicalScope( previous = scopes ) )

    def visitAndCodelet( self, codelet: codetree.AndCodelet, scopes ):
        for c in codelet.members():
            c.visit( self, scopes )

    def visitOrCodelet( self, codelet: codetree.AndCodelet, scopes ):
        for c in codelet.members():
            c.visit( self, scopes )

    def visitFunctionCodelet( self, fun_codelet, scopes ):
        new_scopes = LexicalScope( previous = scopes, lambdaCodelet=fun_codelet )
        fun_codelet.parameters().visit( self, new_scopes )
        fun_codelet.body().visit( self, new_scopes )


def resolveCodeTree( tree ):
    """
    resolveCodeTree updates a code-tree in place.
    """
    tree.visit( Resolver(), GlobalScope() )
   
def resolveFile( file ):
    """
    resolveFile deserialises a file and then resolves it. This is only
    useful when the resolve phase is being used standalone.
    """
    tree = codetree.codeTreeFromJSONFileObject( file )
    resolveCodeTree( tree )
    return tree
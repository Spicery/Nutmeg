import codetree
import abc
from syscalls import isSysconst

class Scope( abc.ABC ):

    @abc.abstractmethod
    def lookup( self, name ):
        pass

    @abc.abstractmethod
    def addInfo( self, code_let ):
        pass

    @abc.abstractmethod
    def declare( self, id_let ):
        pass


class GlobalScope( Scope ):

    def lookup( self, name ):
        """Returns the scope that the name appears in, by definition this is the outermost scope"""
        return self

    def addInfo( self, id_codelet ):
        id_codelet.setAsGlobal()

    def declare( self, id_codelet ):
        id_codelet.setAsGlobal()

LABEL = 0
def newLabel():
    global LABEL
    LABEL = LABEL + 1
    return LABEL

class LexicalScope( Scope ):

    def __init__( self, is_block = False, previous = None ):
        self._previous = previous
        self._is_block = is_block
        self._locals = {}

    def lookup( self, name ):
        """Returns the scope that the name appears in"""
        if name in self._locals:
            return self
        else:
            return self._previous.lookup( name )

    def addInfo( self, code_let ):
        code_let.setAsLocal( **self._locals[ code_let.name() ] )

    def declare( self, id_codelet ):
        nm = id_codelet.name()
        if nm in self._locals:
            raise Exception( f'Trying to re-declare the same variable: {nm}' )
        label = newLabel()
        reftype = id_codelet.reftype()
        nonassignable = reftype == "val" or reftype == "const"
        immutable = reftype == "const"
        info = dict( label = label, nonassignable = nonassignable, immutable = immutable )
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
            scopes.lookup( nm ).addInfo( id_codelet )
        elif reftype == "var" or reftype == "val" or reftype == "const" or reftype == "new":
            scopes.declare( id_codelet )
        else:
            raise Exception( f'Unexpected reftype (Internal error?): {reftype}')

    def visitSeqCodelet( self, codelet, scopes ):
        for c in codelet.members():
            c.visit( self, scopes )

    def visitSyscallCodelet( self, codelet, scopes ):
        for c in codelet.members():
            c.visit( self, scopes )

    def visitCallCodelet( self, codelet, scopes ):
        for c in codelet.members():
            c.visit( self, scopes )

    def visitBindingCodelet( self, binding_codelet, scopes ):
        """
        Fix up bindings e.g.  x := EXPR
        """
        binding_codelet.lhs().visit( self, scopes )
        binding_codelet.rhs().visit( self, scopes )

    def visitIfCodelet( self, if_codelet, scopes ):
        """
        Fix up if-expression e.g. if t then x else y endif
        """
        if_codelet.testPart().visit( self, scopes )
        if_codelet.thenPart().visit( self, LexicalScope( previous = scopes ) )
        if_codelet.elsePart().visit( self, LexicalScope( previous = scopes ) )

    def visitFunctionCodelet( self, fun_codelet, scopes ):
        new_scopes = LexicalScope( previous = scopes )
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
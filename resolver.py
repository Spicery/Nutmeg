import codetree
import abc

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
        return self

    def addInfo( self, code_let ):
        code_let.setAsGlobal()

    def declare( self, id_let ):
        pass

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
        if name in self._locals:
            return self
        else:
            return self._previous.lookup( name )

    def addInfo( self, code_let ):
        code_let.setAsLocal( **self._locals[ code_let.name() ] )

    def declare( self, id_let ):
        nm = id_let.name()
        if nm in self._locals:
            raise Exception( f'Trying to re-declare the same variable: {nm}' )
        label = newLabel()
        info = dict( label=label, reftype=id_let.reftype() )
        self._locals[ nm ] = info
        id_let.declareAsLocal( label )

class Resolver( codetree.CodeletVisitor ):

    def resolveFile( self, file ):
        tree = codetree.deserialise( file )
        return self.resolveCodeTree( tree )

    def resolveCodeTree( self, tree ):
        tree.visit( self, GlobalScope() )

    def visitCodelet( self, code_let, scopes ):
        """
        By default leave the tree alone.
        """
        pass

    def visitIdCodelet( self, code_let, scopes ):
        """
        Fix up variables e.g.  x
        """
        nm = code_let.name()
        scopes.lookup( nm ).addInfo( code_let )

    def visitBindingCodelet( self, code_let, scopes ):
        """
        Fix up bindings e.g.  x := EXPR
        """
        lhs = code_let.lhs()
        assert lhs.KIND == "id"
        scopes.declare( lhs )
        rhs = code_let.rhs()
        rhs.visit( self, scopes )






class Arity:

    def __init__( self, count, more=False ):
        self._count = count
        self._more = more

    @staticmethod
    def fromString( astring ):
        if astring[-1] == "+":
            allbutlast1 = astring[0:-1]
            n = int( allbutlast1 )
            return Arity( n, more=True )
        else:
            return Arity( int(astring ) )

    def toString( self ):
        return f"{self._count}{'+' if self._more else ''}"

    def hasArity( self, count, more=False ):
        return self._count == count and self._more == more

    def sum( self, *others ):
        sofar = Arity( self._count, more=self._more )
        for x in others:
            sofar._count += x._count
            sofar._more = sofar._more or x._more
        return sofar

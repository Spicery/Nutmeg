
class Mishap( Exception ):
    """
    Use this class for raising errors inside the compiler. The top-level
    of the compiler traps mishap exceptions and reports the error message
    and all the keyword arguments that have been passed in.
    """

    def __init__( self, *args, **kwargs ):
        super().__init__( *args )
        self._kwargs_list = [ kwargs ]

    def addDetails( self, **kwargs ):
        self._kwargs_list.append( kwargs )
        return self

    def items( self ):
        for kwargs in self._kwargs_list:
            for (k, v) in kwargs.items():
                yield k, v
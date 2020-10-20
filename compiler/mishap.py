
class Mishap( Exception ):

    def __init__( self, *args, **kwargs ):
        super().__init__( *args )
        self._kwargs_list = [ kwargs ]

    def culprit( self, **kwargs ):
        self._kwargs_list.append( kwargs )

    def items( self ):
        for kwargs in self._kwargs_list:
            for (k, v) in kwargs.items():
                yield k, v
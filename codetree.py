###############################################################################
# CodeTree - the representation of a nutmeg program
###############################################################################

import json
import abc
from str2bool import str2bool

class Codelet( abc.ABC ):
	"""
	This represents a single node of a code-tree. It is an abstract class
	with many subclasses.
	"""

	KIND_PROPERTY = "kind"

	def __init__( self, kind=None, **kwargs ):
		"""
		The keyword-arguments are going to be supplied from the deserialisd
		JSON, so the keywords will match the object-fields from the JSON,
		although the values will be codelets and not plain-JSON objects.
		"""
		self._kwargs = kwargs

	def serialise( self, dst ):
		"""
		Converts a this node into a text stream. We use a custom converter
		which is provided later in this file.
		"""
		json.dump( self, dst, sort_keys=True, indent=4, cls=CodeTreeEncoder )
		print( file=dst )

	@abc.abstractmethod
	def encodeAsJSON( self, encoder ):
		raise Exception( 'Not defined' )


class ConstantCodelet( Codelet ):
	"""
	An abstract class for all codelets that represent literal constants.
	"""

	KIND = None

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, value=self._value, **self._kwargs )


class StringCodelet( ConstantCodelet ):

	KIND = "string"

	def __init__( self, *, value, **kwargs ):
		super().__init__( **kwargs )
		self._value = value


class IntCodelet( ConstantCodelet ):
	KIND = "int"

	def __init__( self, *, value, radix=10, **kwargs ):
		super().__init__( **kwargs )
		self._value = int( value, radix )


class BoolCodelet( ConstantCodelet ):
	KIND = "bool"

	def __init__( self, *, value, **kwargs ):
		super().__init__( **kwargs )
		self._value = str2bool( value )


class IdCodelet( Codelet ):

	KIND = "id"

	def __init__( self, *, name, reftype, **kwargs ):
		super().__init__( **kwargs )
		self._name = name
		self._reftype = reftype

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, name=self._name, reftype=self._reftype, **self._kwargs )

class IfCodelet( Codelet ):

	KIND = "if"

	# Slightly awkward because the constructor for an if-codelet uses a Python
	# reserved word (else) as a keyword-argument.
	def __init__( self, *, test, then, **kwargs ):
		self._else = kwargs.pop( 'else', None )
		super().__init__( **kwargs )
		self._test = test
		self._then = then

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, test=self._test, then=self._then )

class BindingCodelet( Codelet ):

	KIND = "binding"

	def __init__( self, *, lhs, rhs, **kwargs ):
		super().__init__( **kwargs )
		self._lhs = lhs
		self._rhs = rhs

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, lhs=self._lhs, rhs=self._rhs, **self._kwargs )

### Serialisation #############################################################

class CodeTreeEncoder(json.JSONEncoder):
	"""
	This is an extension to the Python's JSON serialiser for code-trees.
	These have to be written as classes that inherit from json.JSONEncoder
	and override the 'default' method.
	"""

	def default( self, obj ):
		if isinstance( obj, Codelet ):
			return obj.encodeAsJSON( self )
		return json.JSONEncoder.default( self, obj )

### Deserialisation ###########################################################

def makeDeserialisationTable():
	"""
	Scans the class hierarchy under CodeTree to find all the leaf classes
	and then adds them to a mapping from kinds to constructors.
	"""
	mapping_table = {}
	list = [ Codelet ]
	while list:
		codetree_class = list.pop()
		subclasses = codetree_class.__subclasses__()
		if subclasses:
			list.extend( subclasses )
		else:
			mapping_table[ codetree_class.KIND ] = codetree_class
	return mapping_table

# This is the master table for driving the deserialisation of code-trees.
DESERIALISATION_TABLE = makeDeserialisationTable()

def codeTreeJSONHook( jdict ):
	'''
	This is an extension method for Python's json deserialiser. It detects
	items that are of the right kind and calls the matching constructor
	on using the JSON object to supply the keyword-parameters.
	'''
	if Codelet.KIND_PROPERTY in jdict:
		e = jdict[Codelet.KIND_PROPERTY ]
		return DESERIALISATION_TABLE[e]( **jdict )
	else:
		return jdict

def deserialise( src ):
	"""
	Reads a text stream in JSON format into a nutmeg-tree.
	"""
	return json.load( src, object_hook=codeTreeJSONHook )

###---###


if __name__ == "__main__":
	B = json.load( open( 'codetree-examples/binding.codetree.json', 'r' ), object_hook=codeTreeJSONHook)

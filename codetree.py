###############################################################################
# CodeTree - the representation of a nutmeg program
###############################################################################

import json
import abc
from abc import ABC, abstractmethod, abstractproperty

from str2bool import str2bool

class CodeletVisitor( abc.ABC ):

	def visitCodelet( self, code_let, *args, **kwargs ):
		raise Exception( 'Unable to handle visitor' )

	def visitConstantCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitStringCodelet( self, code_let, *args, **kwargs ):
		return self.visitConstantCodelet( code_let, *args, **kwargs )

	def visitIntCodelet( self, code_let, *args, **kwargs ):
		return self.visitConstantCodelet( code_let, *args, **kwargs )

	def visitBoolCodelet( self, code_let, *args, **kwargs ):
		return self.visitConstantCodelet( code_let, *args, **kwargs )

	def visitIdCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitSyscallCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitIfCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitSeqCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitBindingCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )



class Codelet( abc.ABC ):
	"""
	This represents a single node of a code-tree. It is an abstract class
	with many subclasses.
	"""

	KIND_PROPERTY = "kind"

	def __init__( self, **kwargs ):
		"""
		The keyword-arguments are going to be supplied from the deserialisd
		JSON, so the keywords will match the object-fields from the JSON,
		although the values will be codelets and not plain-JSON objects.
		"""
		if Codelet.KIND_PROPERTY in kwargs:
			del kwargs[Codelet.KIND_PROPERTY]
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

	def toJSON( self ):
		import io
		stream = io.StringIO()
		self.serialise( stream )
		return json.loads( stream.getvalue() )

	@abc.abstractmethod
	def subExpressions( self ):
		raise Exception( 'Not defined' )

	@abc.abstractmethod
	def visit( self, visitor, *args, **kwargs ):
		raise Exception( 'Not defined' )

class ConstantCodelet( Codelet, ABC ):
	"""
	An abstract class for all codelets that represent literal constants.
	"""

	KIND = None

	def valueAsString( self ):
		return str( self._value )

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, value=self.valueAsString(), **self._kwargs )

	def subExpressions( self ):
		return ()


class StringCodelet( ConstantCodelet ):

	KIND = "string"

	def __init__( self, *args, value = "", **kwargs ):
		super().__init__( **kwargs )
		if args:
			if len(args) == 1:
				self._value = str(args[0])
			else:
				raise Exception( 'Too many arguments for StringCodelet' )
		else:
			self._value = str(value)

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitStringCodelet( self, *args, **kwargs )


class IntCodelet( ConstantCodelet ):
	KIND = "int"

	def __init__( self, *args, value = 0, radix=10, **kwargs ):
		super().__init__( **kwargs )
		if args:
			if len(args) == 1:
				self._value = int(args[0])
			else:
				raise Exception( 'Too many arguments for StringCodelet' )
		else:
			self._value = int( value, radix )

	def visit( self, visitor, *args, **kwargs ):
		print( 'IntCodelet ARGS', args )
		return visitor.visitIntCodelet( self, *args, **kwargs )


class BoolCodelet( ConstantCodelet ):
	KIND = "bool"

	def __init__( self, *args, value = False, **kwargs ):
		super().__init__( **kwargs )
		if args:
			if len(args) == 1:
				self._value = bool(args[0])
			else:
				raise Exception( 'Too many arguments for StringCodelet' )
		else:
			self._value = str2bool( value )

	def valueAsString( self ):
		return 'true' if self._value else 'false'

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitBoolCodelet( self, *args, **kwargs )


class IdCodelet( Codelet ):

	KIND = "id"

	def __init__( self, *, name, reftype, **kwargs ):
		self._scope = kwargs.pop( 'scope', None )
		self._label = kwargs.pop( 'label', None )
		self._nonassignable = kwargs.pop( 'nonassignable', None )
		self._immutable = kwargs.pop( 'immutable', None )
		super().__init__( **kwargs )
		self._name = name
		self._reftype = reftype

	def name( self ):
		return self._name

	def scope( self ):
		return self._scope

	def refype( self ):
		return self._reftype

	def nonassignable( self ):
		return self._nonassignable

	def immutable( self ):
		return self._immutable

	def encodeAsJSON( self, encoder ):
		d = dict( kind=self.KIND, name=self._name, reftype=self._reftype, **self._kwargs )
		if self._scope:
			d[ 'scope' ] = self._scope
		if self._label:
			d[ 'label' ] = self._label
		if self._nonassignable is not None:
			d[ 'nonassignable' ] = self._nonassignable
		if self._immutable is not None:
			d[ 'immutable' ] = self._immutable
		return d

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitIdCodelet( self, *args, **kwargs )

	def setAsGlobal( self ):
		self._scope = "global"

	def setAsLocal( self, **kwargs ):
		self._scope = "local"

	def declareAsLocal( self, *, label, **kwargs ):
		self._scope = "local"
		self._label = label

	def subExpressions( self ):
		return ()

class SyscallCodelet( Codelet ):

	KIND="syscall"

	def __init__( self, *, name, arguments, **kwargs ):
		super().__init__( **kwargs )
		self._name = name
		self._arguments = arguments

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, name=self._name, arguments=self._arguments, **self._kwargs )

	def subExpressions( self ):
		return tuple( self._arguments )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitSyscallCodelet( self, *args, **kwargs )

class IfCodelet( Codelet ):

	KIND = "if"

	# Slightly awkward because the constructor for an if-codelet uses a Python
	# reserved word (else) as a keyword-argument.
	def __init__( self, *, test, then, **kwargs ):
		self._else = kwargs.pop( 'else', None )
		super().__init__( **kwargs )
		self._test = test
		self._then = then

	def testPart( self ):
		return self._test

	def thenPart( self ):
		return self._then

	def elsePart( self ):
		return self._else

	def encodeAsJSON( self, encoder ):
		d = dict( kind=self.KIND, test=self._test, then=self._then )
		d[ 'else' ] = self._else
		return d

	def subExpressions( self ):
		return self._test, self._then, self._else

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitIfCodelet( self, *args, **kwargs )

class SeqCodelet( Codelet ):

	KIND = "seq"
	def __init__( self, *args, body = [], **kwargs ):
		super().__init__( **kwargs )
		self._body = [ *args, *body ]
		
	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, body=self._body )

	def subExpressions( self ):
		return tuple( self._body )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitSeqCodelet( self, *args, **kwargs )
	


	def subExpressions( self ):
		return self._test, self._then, self._else

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitIfCodelet( self, *args, **kwargs )

class SeqCodelet( Codelet ):

	KIND = "seq"
	def __init__( self, *args, body = [], **kwargs ):
		super().__init__( **kwargs )
		self._body = [ *args, *body ]
		
	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, body=self._body )

	def subExpressions( self ):
		return tuple( self._body )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitSeqCodelet( self, *args, **kwargs )
	


class BindingCodelet( Codelet ):

	KIND = "binding"

	def __init__( self, *, lhs, rhs, **kwargs ):
		super().__init__( **kwargs )
		self._lhs = lhs
		self._rhs = rhs

	def lhs( self ):
		return self._lhs

	def rhs( self ):
		return self._rhs

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, lhs=self._lhs, rhs=self._rhs, **self._kwargs )

	def subExpressions( self ):
		return ( self._rhs, )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitBindingCodelet( self, *args, **kwargs )


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

#
# if __name__ == "__main__":
# 	B = json.load( open( 'codetree-examples/binding.codetree.json', 'r' ), object_hook=codeTreeJSONHook)

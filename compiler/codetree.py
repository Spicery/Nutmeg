###############################################################################
# CodeTree - the representation of a nutmeg program
###############################################################################

import io
import json
import abc
from abc import ABC

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

	def visitSysfnCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitCallCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitIfCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitInCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitForCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitSeqCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitBindingCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitAssignCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )

	def visitFunctionCodelet( self, code_let, *args, **kwargs ):
		return self.visitCodelet( code_let, *args, **kwargs )


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

	def serialize( self, dst ):
		"""
		Converts a this node into a text stream. We use a custom converter
		which is provided later in this file.
		"""
		json.dump( self, dst, sort_keys=True, indent=4, cls=CodeTreeEncoder )
		print( file=dst )

	def serializeToString( self ):
		import io
		b = io.StringIO()
		self.serialize( b )
		return b.getvalue()

	@abc.abstractmethod
	def members( self ):
		raise Exception( 'Not defined' )

	@abc.abstractmethod
	def transform( self, f ):
		raise Exception( 'Not defined' )

	@abc.abstractmethod
	def encodeAsJSON( self, encoder ):
		raise Exception( 'Not defined' )

	def toJSON( self ):
		import io
		stream = io.StringIO()
		self.serialize( stream )
		return json.loads( stream.getvalue() )

	@abc.abstractmethod
	def visit( self, visitor, *args, **kwargs ):
		raise Exception( 'Not defined' )

	def declarationMode( self ):
		for m in self.members():
			m.declarationMode()

	def assignMode( self ):
		for m in self.members():
			m.assignMode()

class ConstantCodelet( Codelet, ABC ):
	"""
	An abstract class for all codelets that represent literal constants.
	"""

	KIND = None

	def members( self ):
		yield from ()

	def transform( self, f ):
		return self

	def valueAsString( self ):
		return str( self._value )

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, value=self.valueAsString(), **self._kwargs )


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

	def valueAsBool( self ):
		return self._value

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitBoolCodelet( self, *args, **kwargs )


class IdCodelet( Codelet ):

	KIND = "id"

	def __init__( self, *, name, reftype, slot=None, immutable=None, nonassignable=None, scope=None, label=None, **kwargs ):
		super().__init__( **kwargs )
		self._name = name
		self._reftype = reftype
		self._slot = slot
		self._scope = scope
		self._label = label
		self._nonassignable = nonassignable
		self._immutable = immutable

	def members( self ):
		yield from ()

	def transform( self, f ):
		return self

	def name( self ):
		return self._name

	def scope( self ):
		return self._scope

	def slot( self ):
		return self._slot

	def setSlot( self, n ):
		self._slot = n

	def reftype( self ):
		return self._reftype

	def nonassignable( self ):
		return self._nonassignable

	def immutable( self ):
		return self._immutable

	def label( self ):
		return self._label

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
		if self._slot is not None:
			d[ 'slot' ] = self._slot
		return d

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitIdCodelet( self, *args, **kwargs )

	def setAsGlobal( self ):
		self._scope = "global"
		self._nonassignable = True

	def setAsLocal( self, * , label, nonassignable, immutable, **kwargs ):
		self._scope = "local"
		self._label = label
		self._nonassignable = nonassignable
		self._immutable = immutable

	def declareAsLocal( self, *, label, nonassignable, immutable, **kwargs ):
		self._scope = "local"
		self._reftype = "new"
		self._label = label
		self._nonassignable = nonassignable
		self._immutable = immutable

	def declarationMode( self ):
		if self._reftype == "get":
			self._reftype = "val"

	def assignMode( self ):
		if self._reftype == "get":
			self._reftype = "set"


class SysfnCodelet( Codelet ):

	KIND="sysfn"

	def __init__( self, *, name, **kwargs ):
		super().__init__( **kwargs )
		self._name = name

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, name=self._name, **self._kwargs )

	def members( self ):
		yield from ()

	def transform( self, f ):
		return self

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitSysfnCodelet( self, *args, **kwargs )

class SyscallCodelet( Codelet ):

	KIND="syscall"

	def __init__( self, *, name, arguments, **kwargs ):
		super().__init__( **kwargs )
		self._name = name
		self._arguments = arguments

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, name=self._name, arguments=self._arguments, **self._kwargs )

	def name( self ):
		return self._name

	def arguments( self ):
		return self._arguments

	def members( self ):
		yield self._arguments

	def transform( self, f ):
		return SyscallCodelet( name=self._name, arguments=f( self._arguments ), **self._kwargs )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitSyscallCodelet( self, *args, **kwargs )

class CallCodelet( Codelet ):

	KIND="call"

	def __init__( self, *, function, arguments, **kwargs ):
		super().__init__( **kwargs )
		self._function = function
		self._arguments = arguments

	def function( self ):
		return self._function

	def arguments( self ):
		return self._arguments

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, function=self._function, arguments=self._arguments, **self._kwargs )

	def members( self ):
		yield self._function
		yield self._arguments

	def transform( self, f ):
		return CallCodelet( function=f(self._function), arguments=f(self._arguments), **self._kwargs )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitCallCodelet( self, *args, **kwargs )

class InCodelet( Codelet ):

	KIND = "in"

	def __init__( self, *, pattern, streamable, **kwargs ):
		self._streamSlot = kwargs.pop( "streamSlot", None )
		super().__init__( **kwargs )
		self._pattern = pattern
		self._streamable = streamable

	def pattern( self ):
		return self._pattern

	def streamable( self ):
		return self._streamable

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, pattern=self._pattern, streamable=self._streamable, streamSlot=self._streamSlot, **self._kwargs )

	def members( self ):
		yield self._pattern
		yield self._streamable

	def transform( self, f ):
		return InCodelet( pattern=f(self._pattern), streamable=f(self._streamable), **self._kwargs )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitInCodelet( self, *args, **kwargs )

class ForCodelet( Codelet ):

	KIND = "for"

	def __init__( self, *, query, body, **kwargs ):
		super().__init__( **kwargs )
		self._query = query
		self._body = body

	def query( self ):
		return self._query

	def body( self ):
		return self._body

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, query=self._query, body=self._body, **self._kwargs )

	def members( self ):
		yield self._query
		yield self._body

	def transform( self, f ):
		return ForCodelet( query=f( self._query ), body=f( self._body ), **self._kwargs )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitForCodelet( self, *args, **kwargs )


class IfCodelet( Codelet ):

	KIND = "if"

	# Slightly awkward because the constructor for an if-codelet uses a Python
	# reserved word (else) as a keyword-argument.
	def __init__( self, *, test=None, then=None, testPart=None, thenPart=None, elsePart=None, **kwargs ):
		self._else = kwargs.pop( 'else', elsePart )
		super().__init__( **kwargs )
		self._test = test or testPart
		self._then = then or thenPart
		if self._test is None:
			raise Exception( 'Test part is not specified' )
		if self._then is None:
			raise Exception( 'Then part is not specified' )

	def members( self ):
		yield self.testPart()
		yield self.thenPart()
		yield self.elsePart()

	def transform( self, f ):
		return IfCodelet( 
			test=f( self.testPart() ), 
			then=f( self.thenPart() ), 
			**{
				'else': f(self.elsePart())
			},
			**self._kwargs
		)

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

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitIfCodelet( self, *args, **kwargs )


class SeqCodelet( Codelet ):

	KIND = "seq"
	def __init__( self, *args, body = [], **kwargs ):
		super().__init__( **kwargs )
		self._body = [ *args, *body ]
		
	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, body=self._body )

	def members( self ):
		yield from self._body

	def body( self ):
		return self._body

	def __getitem__(self, item):
		return self._body[ item ]

	def transform( self, f ):
		return SeqCodelet( *map( f, self._body ), **self._kwargs )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitSeqCodelet( self, *args, **kwargs )

class AssignLikeCodelet( Codelet, ABC ):

	def __init__( self, *, lhs, rhs, **kwargs ):
		super().__init__( **kwargs )
		self._lhs = lhs
		self._rhs = rhs

	def members( self ):
		yield self.lhs()
		yield self.rhs()

	def lhs( self ):
		return self._lhs

	def rhs( self ):
		return self._rhs

class AssignCodelet( AssignLikeCodelet ):

	KIND = "assign"

	def transform( self, f ):
		return AssignCodelet( lhs=f( self.lhs() ), rhs=f( self.rhs() ), **self._kwargs )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitAssignCodelet( self, *args, **kwargs )

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, lhs=self._lhs, rhs=self._rhs, **self._kwargs )

class BindingCodelet( AssignLikeCodelet ):

	KIND = "binding"

	def __init__( self, *, lhs, rhs, annotations=None, **kwargs ):
		super().__init__( lhs=lhs, rhs=rhs, **kwargs )
		self._annotations = annotations or {}

	def transform( self, f ):
		return BindingCodelet( lhs=f( self.lhs() ), rhs=f( self.rhs() ), annotations=self._annotations, **self._kwargs )

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitBindingCodelet( self, *args, **kwargs )

	def encodeAsJSON( self, encoder ):
		return dict( kind=self.KIND, lhs=self._lhs, rhs=self._rhs, annotations=self._annotations, **self._kwargs )

	def setAnnotation( self, **kwargs ):
		self._annotations.update( kwargs )

	def annotations( self ):
		return self._annotations


class LambdaCodelet( Codelet ):

	KIND = "lambda"

	def __init__( self, *, parameters, body, nlocals = None, nargs = None, **kwargs ):
		super().__init__( **kwargs )
		self._parameters = parameters
		self._body = body
		self._nlocals = nlocals
		self._nargs = nargs

	def members( self ):
		yield self.parameters()
		yield self.body()

	def transform( self, f ):
		return LambdaCodelet( parameters=f( self._parameters ), body=f( self._body ), **self._kwargs )

	def parameters( self ):
		return self._parameters

	def body( self ):
		return self._body

	def nlocals( self ):
		return self._nlocals

	def setNlocals( self, n : int ):
		self._nlocals = n

	def nargs( self ):
		return self._nargs

	def setNargs( self, n : int ):
		self._nargs = n

	def encodeAsJSON( self, encoder ):
		d = dict( kind=self.KIND, parameters=self._parameters, body=self._body, **self._kwargs )
		if self._nlocals is not None:
			d[ 'nlocals' ] = self._nlocals
		if self._nargs is not None:
			d[ 'nargs' ] = self._nargs
		return d

	def visit( self, visitor, *args, **kwargs ):
		return visitor.visitFunctionCodelet( self, *args, **kwargs )


### Serialisation ##############################################################

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

### Deserialization ###########################################################

def makeDeserializationTable():
	"""
	Scans the class hierarchy under CodeTree to find all the leaf classes
	and then adds them to a mapping from kinds to constructors.
	"""
	mapping_table = {}			# This is the table we are trying to complete.
	L = [ Codelet ]				# Use as a quick and dirty stack.
	while L:
		codetree_class = L.pop()
		subclasses = codetree_class.__subclasses__()
		if subclasses:
			L.extend( subclasses )
		else:
			# Found a leaf class, so add to mapping.
			mapping_table[ codetree_class.KIND ] = codetree_class
	return mapping_table

# This is the master table for driving the deserialisation of code-trees.
DESERIALIZATION_TABLE = makeDeserializationTable()

def codeTreeJSONHook( jdict ):
	"""
	This is an extension method for Python's json deserialiser. It detects
	items that are of the right kind and calls the matching constructor
	on using the JSON object to supply the keyword-parameters.
    """
	if Codelet.KIND_PROPERTY in jdict:
		e = jdict[Codelet.KIND_PROPERTY ]
		return DESERIALIZATION_TABLE[e ]( **jdict )
	else:
		return jdict

def __deserialize( src ):
	"""
	Reads a text stream in JSON format into a nutmeg-tree.
	"""
	return json.load( src, object_hook=codeTreeJSONHook )

def codeTreeFromJSONFileObject( src ):
	return __deserialize( src )

def codeTreeFromJSONString( jstring ):
	return codeTreeFromJSONFileObject( io.StringIO( jstring ) )

def codeTreeFromJSONObject( jobject ):
	# TODO: Is there a better way? Surely there is??
	return codeTreeFromJSONString( json.dumps( jobject ) )

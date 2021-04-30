using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NutmegRunner {

    // See https://stackoverflow.com/questions/8030538/how-to-implement-custom-jsonconverter-in-json-net-to-deserialize-a-list-of-base

    public abstract class JsonCreationConverter<T> : JsonConverter {
        /// <summary>
        /// Create an instance of objectType, based properties in the JSON object
        /// </summary>
        /// <param name="objectType">type of object expected</param>
        /// <param name="jObject">
        /// contents of JSON object that will be deserialized
        /// </param>
        /// <returns></returns>
        protected abstract T Create( Type objectType, JObject jObject );

        public override bool CanConvert( Type objectType ) {
            return typeof( T ).IsAssignableFrom( objectType );
        }

        public override bool CanWrite => false;

        public override object ReadJson( JsonReader reader,
                                        Type objectType,
                                         object existingValue,
                                         JsonSerializer serializer ) {
            // Load JObject from stream
            JObject jObject = JObject.Load( reader );

            // Create target object based on JObject
            T target = this.Create( objectType, jObject );

            // Populate the object properties
            serializer.Populate( jObject.CreateReader(), target );

            return target;
        }
    }

    public class CodeletConverter : JsonCreationConverter<Codelet> {

        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer ) {
            throw new NotImplementedException();
        }

        protected override Codelet Create( Type objectType, JObject jObject ) {
            var kind = jObject["kind"];
            return (kind.ToString()) switch {
                "lambda" => new LambdaCodelet(),
                "syscall" => new SyscallCodelet(),
                "sysfn" => new SysfnCodelet(),
                "sysupdate" => new SysupdateCodelet(),
                "string" => new StringCodelet(),
                "int" => new IntCodelet(),
                "char" => new CharCodelet(),
                "bool" => new BoolCodelet(),
                "if" => new If3Codelet(),
                "and" => new AndCodelet(),
                "or" => new OrCodelet(),
                "call" => new CallCodelet(),
                "seq" => new SeqCodelet(),
                "id" => new IdCodelet(),
                "for" => new ForCodelet(),
                "in" => new InCodelet(),
                "nonstop" => new NonStopCodelet(),
                "do" => new DoCodelet(),
                "wuntil" => new WUntilCodelet(),
                "afterwards" => new AfterwardsCodelet(),
                "binding" => new BindingCodelet(),
                "assign" => new AssignCodelet(),
                _ => throw new NutmegException( $"Unrecognised kind: {kind}" ),
            };
        }

    }

    [JsonConverter( typeof( CodeletConverter ) )]
    public abstract class Codelet {

        [JsonProperty( "arity" )]
        public string Arity { get; set; }

        public Arity GetArity() {
            if (this.Arity == null ) {
                return new Arity( 0, true );
            } else if ( this.Arity.EndsWith("+") ) {
                return new Arity( int.Parse( this.Arity.Substring( 0, this.Arity.Length - 1 ) ), true );
            } else {
                return new Arity( int.Parse( Arity ), false );
            }
        }

        public static Codelet DeserialiseCodelet( string jsonValue ) {
            return JsonConvert.DeserializeObject< Codelet >( jsonValue );
        }

        public abstract Runlet Weave( Runlet continuation, GlobalDictionary g );

        public virtual Runlet Weave1( Runlet continuation, GlobalDictionary g ) {
            Arity a = this.GetArity();
            if ( a.HasExactArity( 1 ) ) {
                return this.Weave( continuation, g );
            } else {
                var runlet = this.Weave( new Unlock1Runlet( continuation ), g );
                return new LockRunlet( runlet );
            }
        }

        public virtual Runlet WeaveLoopInit( Runlet continuation, GlobalDictionary g ) {
            throw new NutmegException( "Cannot iterate over expression" ).Culprit( "Expression", $"{this}" );
        }

        public virtual Runlet WeaveLoopBody( Runlet continuation, GlobalDictionary g ) {
            throw new NutmegException( "Cannot iterate over expression" ).Culprit( "Expression", $"{this}" );
        }

        public virtual Runlet WeaveLoopNext( Runlet okLabel, Runlet failLabel, GlobalDictionary g ) {
            throw new NutmegException( "Cannot iterate over expression" ).Culprit( "Expression", $"{this}" );
        }

    }

    public class ForCodelet : Codelet {

        [JsonProperty( "query" )]
        public Codelet Query { get; set; }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            var loopStartLabel = new JumpRunlet( null );
            var loopInit = this.Query.WeaveLoopInit( loopStartLabel, g );
            var loopBody = this.Query.WeaveLoopBody( loopStartLabel, g );
            var loopNext = this.Query.WeaveLoopNext( loopBody, continuation, g );
            loopStartLabel.UpdateLink( loopNext );
            return loopInit;
        }

    }

    public class DoCodelet : Codelet {

        [JsonProperty( "query" )]
        public Codelet Query { get; set; }

        [JsonProperty( "body" )]
        public Codelet Body { get; set; }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            throw new UnimplementedNutmegException( "Naked 'do' not implemented" );
        }

        public override Runlet WeaveLoopInit( Runlet continuation, GlobalDictionary g ) {
            return Query.WeaveLoopInit( continuation, g );
        }

        public override Runlet WeaveLoopBody( Runlet continuation, GlobalDictionary g ) {
            return Body.Weave( continuation, g );
        }

        public override Runlet WeaveLoopNext( Runlet okLabel, Runlet failLabel, GlobalDictionary g ) {
            return Query.WeaveLoopNext( okLabel, failLabel, g );
        }

    }

    public class NonStopCodelet : Codelet {

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            throw new UnimplementedNutmegException( "Naked 'nonstop' not implemented" );
        }

        public override Runlet WeaveLoopInit( Runlet continuation, GlobalDictionary g ) {
            return continuation;
        }

        public override Runlet WeaveLoopBody( Runlet continuation, GlobalDictionary g ) {
            return continuation;
        }

        public override Runlet WeaveLoopNext( Runlet okLabel, Runlet failLabel, GlobalDictionary g ) {
            return okLabel;
        }


    }

    public class InCodelet : Codelet {

        [JsonProperty( "pattern" )]
        public Codelet InVar { get; set; }

        [JsonProperty( "streamable" )]
        public Codelet InExpr { get; set; }

        [JsonProperty( "streamSlot" )]
        public int InStreamSlot { get; set; }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            throw new UnimplementedNutmegException( "Naked 'in' not implemented" );
        }

        public override Runlet WeaveLoopInit( Runlet continuation, GlobalDictionary g ) {
            var pop = new PopValueIntoSlotRunlet( this.InStreamSlot, continuation );
            var inExpr = this.InExpr.Weave1( new StreamSystemFunction( pop ), g );
            return inExpr;
        }

        public override Runlet WeaveLoopBody( Runlet continuation, GlobalDictionary g ) {
            return continuation;
        }

        public override Runlet WeaveLoopNext( Runlet okLabel, Runlet failLabel, GlobalDictionary g ) {
            IdCodelet inVar = (IdCodelet)this.InVar;
            return new SlotNextRunlet( this.InStreamSlot, new PopSlotRunlet( inVar.Slot, okLabel ), failLabel );
        }

    }

    public class WUntilCodelet : Codelet {

        [JsonProperty( "sense" )]
        public bool Sense { get; set; }

        [JsonProperty( "query" )]
        public Codelet Query { get; set; }

        [JsonProperty( "test" )]
        public Codelet Test { get; set; }

        [JsonProperty( "result" )]
        public Codelet Result { get; set; }

        public bool IsWhile() => this.Sense;

        public bool IsUntil() => !this.Sense;

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            throw new UnimplementedNutmegException( "Naked 'while/until' not implemented" );
        }

        public override Runlet WeaveLoopInit( Runlet continuation, GlobalDictionary g ) {
            return this.Query.WeaveLoopInit( continuation, g );
        }

        public override Runlet WeaveLoopBody( Runlet continuation, GlobalDictionary g ) {
            return this.Query.WeaveLoopBody( continuation, g );
        }

        public override Runlet WeaveLoopNext( Runlet okLabel, Runlet failLabel, GlobalDictionary g ) {
            Runlet resultLabel = this.Result.Weave( failLabel, g );
            Runlet forkLabel = this.IsUntil() ? new ForkRunlet( resultLabel, okLabel ) : new ForkRunlet( okLabel, resultLabel );
            Runlet okLabel1 = this.Test.Weave( forkLabel, g );
            return this.Query.WeaveLoopNext( okLabel1, failLabel, g );
        }

    }

    public class AfterwardsCodelet : Codelet {

        [JsonProperty( "query" )]
        public Codelet Query { get; set; }

        [JsonProperty( "result" )]
        public Codelet Result { get; set; }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            throw new UnimplementedNutmegException( "Naked 'until' not implemented" );
        }

        public override Runlet WeaveLoopInit( Runlet continuation, GlobalDictionary g ) {
            return Query.WeaveLoopInit( continuation, g );
        }

        public override Runlet WeaveLoopBody( Runlet continuation, GlobalDictionary g ) {
            return Query.WeaveLoopBody( continuation, g );
        }

        public override Runlet WeaveLoopNext( Runlet okLabel, Runlet failLabel, GlobalDictionary g ) {
            Runlet resultLabel = Result.Weave( failLabel, g );
            return this.Query.WeaveLoopNext( okLabel, resultLabel, g );
        }

    }

    public class IdCodelet : Codelet {

        [JsonProperty( "name" )]
        public string Name { get; set; }

        [JsonProperty( "reftype" )]
        public string Reftype { get; set; }

        [JsonProperty( "scope" )]
        public string Scope { get; set; }

        [JsonProperty( "slot" )]
        public int Slot { get; set; }

        public IdCodelet() {
            //  Used by deserialisation.
        }

        public IdCodelet( string name, string reftype, string scope = null, int? slot = null ) {
            this.Name = name;
            this.Reftype = reftype;
            this.Scope = scope;
            this.Slot = slot ?? -1;
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            if (this.Scope == "global") {
                Ident ident = g.Get( this.Name );
                if (this.Reftype == "get") {
                    return new PushIdentRunlet( ident, continuation );
                } else {
                    throw new UnimplementedNutmegException( "IdCodelet.1" );
                }
            } else if (this.Scope == "local" && this.Slot >= 0 ) {
                if (this.Reftype == "get") {
                    return new PushSlotRunlet( this.Slot, continuation );
                } else {
                    throw new UnimplementedNutmegException( "IdCodelet.2" );
                }
            } else {
                throw new UnimplementedNutmegException( "IdCodelet.3" );
            }
        }
    }


    public class LambdaCodelet : Codelet {

        [JsonProperty( "nargs" )]
        public int Nargs { get; set; }
        [JsonProperty( "nlocals" )]
        public int Nlocals { get; set; }
        [JsonProperty( "body" )]
        public Codelet Body { get; set; }


        public LambdaCodelet() {
            //  Used by deserialisation.
        }

        public LambdaCodelet( int nargs, int nlocals, Codelet body ) {
            this.Nargs = nargs;
            this.Nlocals = nlocals;
            this.Body = body;
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return new FunctionRunlet( Nargs, Nlocals, this.Body.Weave( new ReturnRunlet(), g ), continuation );
        }

    }



    public class CallCodelet : Codelet {
        [JsonProperty( "function" )]        //  TODO: I propose we change this to "run".
        Codelet Funarg { get; set; }

        [JsonProperty( "arguments" )]
        Codelet Arguments { get; set; }

        public CallCodelet() {
            //  Used by deserialisation.
        }

        public CallCodelet( Codelet f, Codelet a ) {
            this.Funarg = f;
            this.Arguments = a;
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return new LockRunlet( Arguments.Weave( Funarg.Weave1( new CallSRunlet( continuation ), g ), g ) );
        }

    }

    public class SeqCodelet : Codelet {

        [JsonProperty( "body" )]
        Codelet[] Body { get; set; }

        public SeqCodelet() {
            //  Used by deserialisation.
        }

        public SeqCodelet( params Codelet[] body ) {
            this.Body = body;
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            for (int i = Body.Length - 1; i >= 0; i -= 1) {
                Codelet codelet = Body[i];
                continuation = codelet.Weave( continuation, g );
            }
            return continuation;
        }

    }

    public abstract class ShortCircuitCodelet : Codelet {
        [JsonProperty( "lhs" )]
        public Codelet LeftPart { get; set; }

        [JsonProperty( "rhs" )]
        public Codelet RightPart { get; set; }
    }

    public class AndCodelet : ShortCircuitCodelet {

        public AndCodelet() {
            //  Used by deserialisation
        }

        public AndCodelet( Codelet leftPart, Codelet rightPart ) {
            this.LeftPart = leftPart;
            this.RightPart = rightPart;
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return LeftPart.Weave( new AndRunlet( RightPart.Weave( continuation, g ), continuation ), g );
        }

    }

    public class OrCodelet : ShortCircuitCodelet {

        public OrCodelet() {
            //  Used by deserialisation
        }

        public OrCodelet( Codelet leftPart, Codelet rightPart ) {
            LeftPart = leftPart;
            RightPart = rightPart;
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return LeftPart.Weave( new OrRunlet( RightPart.Weave( continuation, g ), continuation ), g );

        }

    }


    public class If3Codelet : Codelet {

        [JsonProperty( "test" )]
        Codelet TestPart { get; set; }

        [JsonProperty( "then" )]
        Codelet ThenPart { get; set; }

        [JsonProperty( "else" )]
        Codelet ElsePart { get; set; }

        public If3Codelet() {
            //  Used by deserialisation
        }

        public If3Codelet( Codelet testPart, Codelet thenPart, Codelet elsePart ) {
            TestPart = testPart;
            ThenPart = thenPart;
            ElsePart = elsePart;
        }


        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return this.TestPart.Weave( new ForkRunlet( ThenPart.Weave( continuation, g ), ElsePart.Weave( continuation, g ) ), g );
        }

    }

    public abstract class AssignLikeCodelet : Codelet {

        public Codelet LHS { get; set; }
        public Codelet RHS { get; set; }

        public AssignLikeCodelet() {
            //  Used by deserialisation.
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            switch (this.LHS) {
                case IdCodelet lhs_id:
                    var c4 = new PopValueIntoSlotRunlet( lhs_id.Slot, continuation );
                    return this.RHS.Weave1( c4, g );
                default:
                    throw new NutmegException( "Left hand side of binding not a simple variable" );
            }
        }

    }

    public class AssignCodelet : AssignLikeCodelet {
    }

    public class BindingCodelet : AssignLikeCodelet {
    }

    public class UpdateCodelet : Codelet {

        [JsonProperty( "function" )]        //  TODO: I propose we change this to "run".
        Codelet Funarg { get; set; }

        [JsonProperty( "arguments" )]
        Codelet Arguments { get; set; }

        [JsonProperty( "values" )]
        Codelet Values { get; set; }

        public UpdateCodelet() {
            //  Used by deserialisation.
        }

        public UpdateCodelet( Codelet f, Codelet a, Codelet v ) {
            this.Funarg = f;
            this.Arguments = a;
            this.Values = v;
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return new LockRunlet(
                this.Arguments.Weave(
                    new LockRunlet( this.Values.Weave( this.Funarg.Weave1( new UpdateSRunlet( continuation ), g ), g ) ),
                    g
                )
            );
        }

    }

    public class SysfnCodelet : Codelet {

        string _name;
        [JsonProperty( "name" )]
        public string Name {
            get { return _name; }
            set {
                _systemFunction = NutmegSystem.Find( value );
                _name = value;
            }
        }
        private SystemFunctionMaker _systemFunction;

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return this._systemFunction( null );
        }

    }

    public class SyscallCodelet : Codelet {

        string _name;
        [JsonProperty( "name" )]
        public string Name {
            get { return _name; }
            set {
                _systemFunction = NutmegSystem.Find( value );
                _name = value;
            }
        }

        private SystemFunctionMaker _systemFunction;

        [JsonProperty( "arguments" )]
        public Codelet Arguments { get; set; }

        public SyscallCodelet() {
            //  Used by deserialisation.
        }

        /// <summary>
        /// The goal is to take the burden of un/locking the value-stack away from
        /// system functions. So we compile the arguments on a locked stack and
        /// arity check them, unlock the stack, and launch into the system-function
        /// which we may assume is correctly coded.
        /// </summary>
        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            var sysfn = this._systemFunction( continuation );
            switch (sysfn) {
                //  TODO: These should be methods of the system-function object. The natural
                //  way of doing this is to implement SystemFunctionFactories - but that seems
                //  like overkill right now.
                case FixedAritySystemFunction f_sysfn:
                    var nargs = f_sysfn.Nargs;
                    Arity a = this.Arguments.GetArity();
                    //Console.WriteLine( $"Weave Syscall: nargs={nargs}, arity={a.AsString()}" );
                    if ( a.HasExactArity( nargs )) {
                        return this.Arguments.Weave( f_sysfn, g );
                    } else {
                        return new LockRunlet( this.Arguments.Weave( new CheckedUnlockRunlet( nargs, f_sysfn ), g ) );
                    }
                case VariadicSystemFunction v_sysfn:
                    //  If nargs is null then we must count the arguments.
                    var altsysfn = new CountAndUnlockRunlet( this._systemFunction( continuation ) );
                    return new LockRunlet( this.Arguments.Weave( altsysfn, g ) ); 
                default:
                    throw new UnreachableNutmegException();
            }
        }

    }

    public class SysupdateCodelet : Codelet {

        string _name;
        [JsonProperty( "name" )]
        public string Name {
            get { return _name; }
            set {
                _systemFunction = NutmegSystem.Find( value );
                _name = value;
            }
        }

        private SystemFunctionMaker _systemFunction;

        [JsonProperty( "lhs" )]
        public Codelet LHS { get; set; }

        [JsonProperty( "rhs" )]
        public Codelet RHS { get; set; }

        public SysupdateCodelet() {
            //  Used by deserialisation.
        }

        /// <summary>
        /// The goal is to take the burden of un/locking the value-stack away from
        /// system functions. So we compile the arguments on a locked stack and
        /// arity check them, unlock the stack, and launch into the system-function
        /// which we may assume is correctly coded.
        /// </summary>
        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            var sysfn = this._systemFunction( continuation );
            switch (sysfn) {
                //  TODO: These should be methods of the system-function object. The natural
                //  way of doing this is to implement SystemFunctionFactories - but that seems
                //  like overkill right now.
                case IFixedAritySystemUpdater f_sysfn:
                    //Console.WriteLine( "ALPHA" );
                    var ( nargs, unargs ) = f_sysfn.UNargs;
                    Arity a = this.LHS.GetArity();
                    Arity b = this.RHS.GetArity();
                    if ( a.HasExactArity( nargs ) && b.HasExactArity( unargs ) ) {
                        //Console.WriteLine( "ALPHA.1" );
                        return this.LHS.Weave( this.RHS.Weave( new UpdateSystemFunctionRunlet( sysfn ), g ), g );
                    } else {
                        //Console.WriteLine( "ALPHA.2" );
                        var c0 = new CheckedDoubleUnlockRunlet( nargs, unargs, new UpdateSystemFunctionRunlet( sysfn ) );
                        return new LockRunlet( this.LHS.Weave( new LockRunlet( this.RHS.Weave( c0, g ) ), g ) );
                    }
                default:
                    //Console.WriteLine( "BETA" );
                    var d0 = new CountAndDoubleUnlockRunlet( new UpdateSystemFunctionRunlet( sysfn ) );
                    return new LockRunlet( this.LHS.Weave( new LockRunlet( this.RHS.Weave( d0, g ) ), g ) );
            }
        }

    }


    public class StringCodelet : Codelet {

        public StringCodelet() {
            //  Used by deserialisation.
        }

        public StringCodelet( string value ) {
            this.Value = value;
        }

        [JsonProperty( "value" )]
        public string Value { get; set; }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return new PushQRunlet( this.Value, continuation );
        }

    }

    public class IntCodelet : Codelet {

        public IntCodelet() {
            //  Used by deserialisation.
        }

        public IntCodelet( string value ) {
            this.Value = value;
        }

        [JsonProperty( "value" )]
        public string Value { get; set; }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            if (long.TryParse( this.Value, out var n )) {
                return new PushQRunlet( n, continuation );
            } else {
                throw new Exception( $"Invalid int value: {Value}" );
            }
        }

    }

    public class CharCodelet : Codelet {

        public CharCodelet() {
            //  Used by deserialisation.
        }

        public CharCodelet( string value ) {
            this.Value = value;
        }

        [JsonProperty( "value" )]
        public string Value { get; set; }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            if (Char.TryParse( this.Value, out var n )) {
                return new PushQRunlet( n, continuation );
            } else {
                throw new Exception( $"Invalid char value: {Value}" );
            }
        }

    }

    public class BoolCodelet : Codelet {

        public BoolCodelet() {
            //  Used by deserialisation.
        }

        public BoolCodelet( bool value ) {
            this.Value = value;
        }

        [JsonProperty( "value" )]
        public bool Value { get; set; }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return new PushQRunlet( this.Value, continuation );
        }

    }

}

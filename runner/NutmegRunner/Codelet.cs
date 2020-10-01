using System;
using System.Collections.Generic;
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
            switch (kind.ToString()) {
                case "function": return new FunctionCodelet();
                case "syscall": return new SyscallCodelet();
                case "string": return new StringCodelet();
                case "int": return new IntCodelet();
                case "bool": return new BoolCodelet();
                case "if": return new If3Codelet();
                case "call": return new CallCodelet();
                case "seq": return new SeqCodelet();
                case "id": return new IdCodelet();
                case "for": return new ForCodelet();
                case "in": return new InCodelet();
                default: throw new NutmegException( $"Unrecognised kind: {kind}" );
            }
        }

    }

    [JsonConverter( typeof( CodeletConverter ) )]
    public abstract class Codelet {

        public static Codelet DeserialiseCodelet( string jsonValue ) {
            return JsonConvert.DeserializeObject< Codelet >( jsonValue );
        }

        public abstract Runlet Weave( Runlet continuation, GlobalDictionary g );

        public virtual Runlet Weave1( Runlet continuation, GlobalDictionary g ) {
            var runlet = this.Weave( new Unlock1Runlet( continuation ), g );
            return new LockRunlet( runlet );
        }

        public virtual Runlet WeaveLoopInit( Runlet continuation, GlobalDictionary g ) {
            throw new NutmegException( "Cannot iterate over expression" );
        }

        public virtual Runlet WeaveLoopNext( Runlet okLabel, Runlet failLabel, GlobalDictionary g ) {
            throw new NutmegException( "Cannot iterate over expression" );
        }

    }

    public class ForCodelet : Codelet {

        [JsonProperty( "query" )]
        public Codelet Query { get; set; }

        [JsonProperty( "body" )]
        public Codelet Body { get; set; }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            var loopStartLabel = new JumpRunlet( null );
            var loopInit = this.Query.WeaveLoopInit( loopStartLabel, g );
            var loopBody = this.Body.Weave( loopStartLabel, g );
            var loopNext = this.Query.WeaveLoopNext( loopBody, continuation, g );
            loopStartLabel.UpdateLink( loopNext );
            return loopInit;
        }

    }

    public class InCodelet : Codelet {

        [JsonProperty( "in.var" )]
        public Codelet InVar { get; set; }

        [JsonProperty( "in.expr" )]
        public Codelet InExpr { get; set; }

        [JsonProperty( "in.stream.slot" )]
        public int InStreamSlot { get; set; }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            throw new UnimplementedNutmegException( "In not implemented"  );
        }

        public override Runlet WeaveLoopInit( Runlet continuation, GlobalDictionary g ) {
            var pop = new PopValue1IntoSlotRunlet( this.InStreamSlot, continuation );
            var inExpr = this.InExpr.Weave1( new StreamSystemFunction( pop ), g );
            return inExpr;
        }

        public override Runlet WeaveLoopNext( Runlet okLabel, Runlet failLabel, GlobalDictionary g ) {
            IdCodelet inVar = (IdCodelet)this.InVar;
            return new SlotNextRunlet( this.InStreamSlot, new PopSlotRunlet( inVar.Slot, okLabel ), failLabel );
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
                Ident ident = g.Get( Name );
                if (this.Reftype == "get") {
                    return new PushIdentRunlet( ident, continuation );
                } else {
                    throw new UnimplementedNutmegException( "IdCodelet 1" );
                }
            } else if (this.Scope == "local" && this.Slot >= 0 ) {
                if (this.Reftype == "get") {
                    return new PushSlotRunlet( this.Slot, continuation );
                } else {
                    throw new UnimplementedNutmegException( "IdCodelet 2" );
                }
            } else {
                throw new UnimplementedNutmegException( "IdCodelet 3" );
            }
        }
    }


    public class FunctionCodelet : Codelet {

        [JsonProperty( "nargs" )]
        public int Nargs { get; set; }
        [JsonProperty( "nlocals" )]
        public int Nlocals { get; set; }
        [JsonProperty( "body" )]
        public Codelet Body { get; set; }


        public FunctionCodelet() {
            //  Used by deserialisation.
        }

        public FunctionCodelet( int nargs, int nlocals, Codelet body ) {
            this.Nargs = nargs;
            this.Nlocals = nlocals;
            this.Body = body;
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return new FunctionRunlet( Nargs, Nlocals, this.Body.Weave( new ReturnRunlet(), g ), continuation );
        }

    }

    public class CallCodelet : Codelet{
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
            return new LockRunlet( Arguments.Weave( Funarg.Weave( new CallSRunlet( continuation ), g ), g ) );
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
            return TestPart.Weave( new ForkRunlet( ThenPart.Weave( continuation, g ), ElsePart.Weave( continuation, g ) ), g );
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
                    return new LockRunlet( this.Arguments.Weave( new CheckedUnlockRunlet( nargs, f_sysfn ), g ) );
                case VariadicSystemFunction v_sysfn:
                    //  If nargs is null then we cannot unlock the stack until after it runs. But
                    //  we can forgo the value-stack length check.
                    var altsysfn = this._systemFunction( new UnlockRunlet( continuation ) );
                    return new LockRunlet( this.Arguments.Weave( altsysfn, g ) );
                default:
                    throw new UnreachableNutmegException();
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
            if (long.TryParse( this.Value, out var n ) ) {
                return new PushQRunlet( n, continuation );
            } else {
                throw new Exception( $"Invalid int value: {Value}" );
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

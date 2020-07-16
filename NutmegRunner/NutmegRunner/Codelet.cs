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
                default: throw new NutmegException( $"Unrecognised kind: {kind}" );
            }
        }

    }

    [JsonConverter( typeof( CodeletConverter ) )]
    public abstract class Codelet {

        public static Codelet DeserialiseCodelet( string jsonValue ) {
            return JsonConvert.DeserializeObject<FunctionCodelet>( jsonValue );
        }

        public abstract Runlet Weave( Runlet continuation, GlobalDictionary g );

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
            Name = name;
            Reftype = reftype;
            Scope = scope;
            Slot = slot ?? -1;
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            if (Scope == "global") {
                Ident ident = g.Get( Name );
                if (Reftype == "get") {
                    return new PushIdentRunlet( ident, continuation );
                } else {
                    throw new UnimplementedNutmegException();
                }
            } else {
                throw new UnimplementedNutmegException();
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
        [JsonProperty( "function" )]
        Codelet Function { get; set; }

        [JsonProperty( "arguments" )]
        Codelet Arguments { get; set; }

        public CallCodelet() {
            //  Used by deserialisation.
        }

        public CallCodelet( Codelet f, Codelet a ) {
            this.Function = f;
            this.Arguments = a;
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return Arguments.Weave( Function.Weave( new CallSRunlet( continuation ), g ), g);
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
            return TestPart.Weave( new ForkWovenCodelet( ThenPart.Weave( continuation, g ), ElsePart.Weave( continuation, g ) ), g );
        }

    }

    public class SyscallCodelet : Codelet {

        string _name;
        [JsonProperty( "name" )]
        public string Name {
            get { return _name; }
            set {
                _systemFunction = System.Find( value );
                _name = value;
            }
        }

        private SystemFunctionMaker _systemFunction;

        [JsonProperty( "arguments" )]
        public Codelet Arguments { get; set; }

        public SyscallCodelet() {
            //  Used by deserialisation.
        }

        public override Runlet Weave( Runlet continuation, GlobalDictionary g ) {
            return Arguments.Weave( _systemFunction( continuation ), g );
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

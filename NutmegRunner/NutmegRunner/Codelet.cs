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
                case "seq": return new SeqCodelet();
                default: throw new NutmegException( $"Unrecognised kind: {kind}" );
            }
        }

    }

    [JsonConverter( typeof( CodeletConverter ) )]
    public abstract class Codelet {

        public static Codelet DeserialiseCodelet( string jsonValue ) {
            return JsonConvert.DeserializeObject<FunctionCodelet>( jsonValue );
        }

        public abstract Runlet Weave( Runlet continuation );

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

        public override Runlet Weave( Runlet continuation ) {
            return new FunctionRunlet( Nargs, Nlocals, this.Body.Weave( new ReturnRunlet() ) );
        }


    }

    public class SeqCodelet : Codelet {

        [JsonProperty( "body" )]
        Codelet[] Body { get; set; }

        public SeqCodelet() {
            //  Used by deserialisation.
        }

        public SeqCodelet( params Codelet[] body ) {
            Body = body;
        }

        public override Runlet Weave( Runlet continuation ) {
            for (int i = Body.Length - 1; i >= 0; i -= 1) {
                Codelet codelet = Body[i];
                continuation = codelet.Weave( continuation );
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


        public override Runlet Weave( Runlet continuation ) {
            return TestPart.Weave( new ForkWovenCodelet( ThenPart.Weave( continuation ), ElsePart.Weave( continuation ) ) );
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

        public override Runlet Weave( Runlet continuation ) {
            return Arguments.Weave( _systemFunction( continuation ) );
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

        public override Runlet Weave( Runlet continuation ) {
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

        public override Runlet Weave( Runlet continuation ) {
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

        public override Runlet Weave( Runlet continuation ) {
            return new PushQRunlet( this.Value, continuation );
        }

    }

}

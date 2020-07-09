using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NutmegRunner {


    public delegate void SystemFunction( RuntimeEngine runtimeEngine );

    public class System {

        static readonly Dictionary<string, SystemFunction> SYSTEM_FUNCTION_TABLE = new Dictionary<string, SystemFunction>() {
            { "println",  x => Console.WriteLine( $"{x.Pop()}" )  }
        };

        public static SystemFunction Find( string name ) {
            return SYSTEM_FUNCTION_TABLE.TryGetValue( name, out var value ) ? value : null;
        }

    }

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
                default: throw new NutmegException( $"Unrecognised kind: {kind}" );
            }
        }

    }

    [JsonConverter( typeof( CodeletConverter ) )]
    public abstract class Codelet {

        public static Codelet DeserialiseCodelet( string jsonValue ) {
            return JsonConvert.DeserializeObject<FunctionCodelet>( jsonValue );
        }

        public abstract void Evaluate( RuntimeEngine runtimeEngine );

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
            Nargs = nargs;
            Nlocals = nlocals;
            Body = body;
        }

        public override void Evaluate( RuntimeEngine runtimeEngine ) {
            this.Body.Evaluate( runtimeEngine );
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

        private SystemFunction _systemFunction;

        [JsonProperty( "arguments" )]
        public Codelet[] Arguments { get; set; }

        public SyscallCodelet() {
            //  Used by deserialisation.
        }

        public override void Evaluate( RuntimeEngine runtimeEngine ) {
            foreach ( var arg in Arguments ) {
                arg.Evaluate( runtimeEngine );
            }
            this._systemFunction( runtimeEngine );
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

        public override void Evaluate( RuntimeEngine runtimeEngine ) {
            runtimeEngine.Push( this.Value );
        }

    }

}

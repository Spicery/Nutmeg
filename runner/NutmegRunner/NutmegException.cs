using System;
using System.Collections.Generic;

namespace NutmegRunner {

    public class NutmegException : ApplicationException {

        protected Dictionary<string, string> _culprits = new Dictionary<string, string>();

        public IEnumerable<KeyValuePair<string, string>> Culprits => this._culprits;

        public NutmegException Culprit( string name, string value ) {
            this._culprits.Add( name, value );
            return this;
        }

        private NutmegException() : base() {
        }

        public NutmegException( string message ) : base( message ) {
        }

        public NutmegException( string message, Exception exn ) : base( message, exn ) {
        }

        public NutmegException Hint(string v)
        {
            return this.Culprit( "Hint", v );
        }

    }

    public class UsageNutmegException : NutmegException {
        public UsageNutmegException() : base( "Usage" ) {
        }
    }

    public class UnreachableNutmegException : NutmegException {
        public UnreachableNutmegException() : base( "Unreachable" ) {
        }
    }

    public class UnimplementedNutmegException : NutmegException {
        public UnimplementedNutmegException() : base( "Unimplemented" ) {
        }

        public UnimplementedNutmegException( string message ) : base( message ) {
        }
    }

    public class NormalExitNutmegException : NutmegException {
        public NormalExitNutmegException() : base( "NormalExit" ) {
        }
    }

}

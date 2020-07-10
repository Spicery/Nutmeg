using System;
using System.Collections.Generic;

namespace NutmegRunner {

    public class NutmegException : ApplicationException {
        protected Dictionary<string, string> _culprits = new Dictionary<string, string>();

        public IEnumerator<KeyValuePair<string, string>> Culprits => this._culprits.GetEnumerator();

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
    }

    public class UsageNutmegException : NutmegException {
        public UsageNutmegException() : base( "Usage" ) {
        }
    }

    public class NormalExitNutmegException : NutmegException {
        public NormalExitNutmegException() : base( "NormalExit" ) {
        }
    }

}

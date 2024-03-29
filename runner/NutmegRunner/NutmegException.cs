﻿using System;
using System.Collections.Generic;

namespace NutmegRunner {

    public class AssertionFailureException : NutmegException {

        public string Unit { get; set; }
        public long Position { get; set; }

        public AssertionFailureException( string message, object unit, object position, Exception exn = null ) : base( message, exn ) {
            switch ( position ) {
                case long posn:
                    this.Position = posn;
                    break;
            }
            switch (unit) {
                case string u:
                    this.Unit = u;
                    break;
            }
        }
    }

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

        public void WriteMessage() {
            Console.Error.WriteLine( $"MISHAP: {this.Message}" );
            foreach (var culprit in this.Culprits) {
                Console.Error.WriteLine( $" {culprit.Key}: {culprit.Value}" );
            }
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
            this.Hint( "Unimplemented" );
        }

    }

    public class NormalExitNutmegException : NutmegException {
        public NormalExitNutmegException() : base( "NormalExit" ) {
        }
    }

}

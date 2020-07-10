using System;
using System.Collections.Generic;
using System.IO;

namespace NutmegRunner {

    public class RuntimeEngine {

        Dictionary<string, Codelet> _dictionary = new Dictionary<string, Codelet>();
        Stack<object> _valueStack = new Stack<object>();
        Stack<object> _callStack = new Stack<object>();

        public RuntimeEngine() {
        }

        public void Push( string value ) {
            this._valueStack.Push( value );
        }

        public object Pop() {
            return this._valueStack.Pop();
        }

        public object PeekOrElse(object orElse = null) {
            return this._valueStack.TryPeek( out var top ) ? top : orElse;
        }

        public int ValueStackLength() {
            return this._valueStack.Count;
        }

        public void Bind(string idName, Codelet codelet)
        {
            this._dictionary.Add( idName, codelet );
        }

        public void WeaveDictionary() {
            foreach ( var kvp in this._dictionary ) {
                kvp.Value.Weave( null );
            }
        }

        public Codelet Return() {
            return (Codelet)this._callStack.Pop();
        }

        private Codelet SetupStart( FunctionCodelet fc ) {
            CallQCodelet callq = new CallQCodelet( fc );
            callq.ShallowWeave( new HaltCodelet() );
            return callq;
        }

        public void Start( string idName, bool useEvaluate, bool debug ) {
            TextWriter stdErr = Console.Error;
            Codelet codelet = this._dictionary.TryGetValue( idName, out var c ) ? c : null;
            if ( codelet is FunctionCodelet fc ) {
                if (debug) stdErr.WriteLine( $"Running codelet ..." );
                try {
                    if (useEvaluate) {
                        fc.Evaluate( this );
                    } else {
                        Codelet currentInstruction = this.SetupStart( fc );
                        while (true) {
                            //Console.WriteLine( $"current instruction is {currentInstruction}" );
                            currentInstruction = currentInstruction.RunWovenCodelets( this );
                        }
                    }
                } catch (NormalExitNutmegException) {
                    //  Normal exit.
                } finally {
                    if (debug) stdErr.WriteLine( $"Bye, bye ..." );
                }
            } else {
                stdErr.WriteLine( "Entry point {id} is not a function" );
            }
        }

        public void PushReturnAddress( Codelet next ) {
            this._callStack.Push( next );
        }
    }

}

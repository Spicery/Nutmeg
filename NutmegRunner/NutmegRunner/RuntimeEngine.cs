using System;
using System.Collections.Generic;
using System.IO;

namespace NutmegRunner {

    public interface ILayeredStack {
        public void Push( string value );
        public object Pop();
        public bool IsEmpty();
        public int Size();

        public int Lock();
        public int Unlock();
        public int LockCount();

        public object Peek();
        public object PeekOrElse( object orElse = null );
        public object PeekItem( int n );
        public object PeekItemOrElse( int n, object orElse = null );
    }

    public class RuntimeEngine {

        Dictionary<string, WovenCodelet> _dictionary = new Dictionary<string, WovenCodelet>();

        /// <summary>
        /// Soon we will need to replace this with a custom implementation of a layered stack
        /// with the ability to address from the bottom of the current frame.
        /// </summary>
        Stack<object> _callStack = new Stack<object>();

        /// <summary>
        /// Soon we will need to replace this with a custom implementation of a layered stack.
        /// The emphasis is on efficient pushing and popping from the top of the stack.
        /// </summary>
        Stack<object> _valueStack = new Stack<object>();

        public RuntimeEngine() {
        }

        public void Push( object value ) {
            this._valueStack.Push( value );
        }

        public object Pop() {
            return this._valueStack.Pop();
        }

        public bool PopBool() {
            return (Boolean)this._valueStack.Pop();
        }

        public object PeekOrElse(object orElse = null) {
            return this._valueStack.TryPeek( out var top ) ? top : orElse;
        }

        public object[] ValueStackAsArrayWithTop0() {
            return this._valueStack.ToArray();
        }

        public int ValueStackLength() {
            return this._valueStack.Count;
        }

        public void Bind( string idName, Codelet codelet ) {
            this._dictionary.Add( idName, codelet.Weave( null ) );
        }

        public WovenCodelet Return() {
            return (WovenCodelet)this._callStack.Pop();
        }

        public void Start( string idName, bool useEvaluate, bool debug ) {
            WovenCodelet codelet = this._dictionary.TryGetValue( idName, out var c ) ? c : null;
            StartFromCodelet( codelet, useEvaluate, debug );
        }

        public void StartFromCodelet( WovenCodelet codelet, bool useEvaluate, bool debug ) {
            TextWriter stdErr = Console.Error;
            if (codelet is FunctionWovenCodelet fwc) {
                if (debug) stdErr.WriteLine( $"Running codelet ..." );
                try {
                    WovenCodelet currentInstruction = new CallQWovenCodelet( fwc, new HaltWovenCodelet() );
                    while (true) {
                        //Console.WriteLine( $"current instruction is {currentInstruction}" );
                        currentInstruction = currentInstruction.RunWovenCodelets( this );
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

        public void PushReturnAddress( WovenCodelet next ) {
            this._callStack.Push( next );
        }


    }

}

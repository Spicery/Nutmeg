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

    public class Ident {
        public object Value { get; set; }

        public Ident( object value ) {
            Value = value;
        }
    }

    public class GlobalDictionary {

        Dictionary<string, Ident> _dictionary = new Dictionary<string, Ident>();

        public void Add( string varName, object value ) {
            if ( this._dictionary.TryGetValue( varName, out var ident ) ) {
                ident.Value = value;
            } else {
                this._dictionary.Add( varName, new Ident( value ) );
            }
        }

        public Ident Get( string idName ) {
            if ( this._dictionary.TryGetValue( idName, out var ident ) ) {
                return ident;
            } else {
                throw new NutmegException( $"Unknown identifier: {idName}" );
            }
        }

    }

    public class RuntimeEngine {

        GlobalDictionary _dictionary = new GlobalDictionary();

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

        public bool Debug { get; }

        public RuntimeEngine( bool debug ) {
            this.Debug = debug;
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
            this._dictionary.Add( idName, codelet.Weave( null, this._dictionary ) );
        }

        public void PreBind( string idName ) {
            this._dictionary.Add( idName, null );
        }

        public Runlet Return() {
            return (Runlet)this._callStack.Pop();
        }

        public void Start( string idName, bool useEvaluate ) {
            Runlet codelet = (Runlet)(this._dictionary.Get( idName ).Value );
            StartFromCodelet( codelet, useEvaluate );
        }

        public void StartFromCodelet( Runlet codelet, bool useEvaluate ) {
            TextWriter stdErr = Console.Error;
            if (codelet is FunctionRunlet fwc) {
                if (Debug) stdErr.WriteLine( $"Running codelet ..." );
                try {
                    Runlet currentInstruction = new CallQRunlet( fwc, new HaltRunlet() );
                    if (Debug) {
                        while (true) {
                            Console.WriteLine( $"Instruction: {currentInstruction}" );
                            currentInstruction = currentInstruction.ExecuteRunlet( this );
                        }
                    } else {
                        while (true) {
                            currentInstruction = currentInstruction.ExecuteRunlet( this );
                        }
                    }
                } catch (NormalExitNutmegException) {
                    //  Normal exit.
                } finally {
                    if (Debug) stdErr.WriteLine( $"Bye, bye ..." );
                }
            } else {
                stdErr.WriteLine( "Entry point {id} is not a function" );
            }
        }

        public void PushReturnAddress( Runlet next ) {
            this._callStack.Push( next );
        }


    }

}

using System;
using System.Collections.Generic;
using System.IO;

namespace NutmegRunner {


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
        UncheckedLayeredStack<object> _callStack = new UncheckedLayeredStack<object>();



        /// <summary>
        /// Soon we will need to replace this with a custom implementation of a layered stack.
        /// The emphasis is on efficient pushing and popping from the top of the stack.
        /// </summary>
        CheckedLayeredStack<object> _valueStack = new CheckedLayeredStack<object>();

        public bool Debug { get; }

        public RuntimeEngine( bool debug ) {
            this.Debug = debug;
        }

        public void LockValueStack() {
            this._valueStack.Lock();
        }

        public void UnlockValueStack() {
            this._valueStack.Unlock();
        }

        public void PushSlot( int slot ) {
            this._valueStack.Push( this._callStack[slot] );
        }

        public void Push( object value ) {
            this._valueStack.Push( value );
        }

        public object Pop() {
            return this._valueStack.Pop();
        }

        public object Pop1() {
            if ( this._valueStack.Size() == 1 ) {
                return this._valueStack.Pop();
            } else if ( this._valueStack.IsEmpty() ) {
                throw new NutmegException( "Required value is missing" ).Hint( "Exactly one value needed but none supplied" );
            } else {
                throw new NutmegException( "Too many values" ).Hint( "Exactly one value needed but too many are supplied" );
            }
        }

        public bool TryPop( out object d ) {
            //  TODO: This can be made faster by handling exceptions.
            if (this._valueStack.IsEmpty()) {
                d = 0;
                return false;
            } else {
                d = this._valueStack.Pop();
                return true;
            }
        }

        public int CreateFrameAndCopyValueStack( int nlocals ) {
            return this._callStack.RawLock( nlocals, this._valueStack );
        }

        public bool PopBool() {
            return (Boolean)this._valueStack.Pop();
        }

        public object PeekOrElse(object orElse = null) {
            return this._valueStack.PeekOrElse( orElse: orElse );
        }

        public int ValueStackLength() {
            return this._valueStack.Size();
        }

        public void Bind( string idName, Codelet codelet ) {
            this._dictionary.Add( idName, codelet.Weave( null, this._dictionary ) );
        }

        public void PreBind( string idName ) {
            this._dictionary.Add( idName, null );
        }

        public Runlet Return() {
            this._valueStack.Unlock();
            this._callStack.ClearAndUnlock();
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
                    Runlet currentInstruction = new LockRunlet( new CallQRunlet( fwc, new HaltRunlet() ) );
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

        public void ShowFrames() {
            Console.WriteLine( $"Current frame has {this._callStack.Size()} slots" );
            for ( int i = 0; i < this._callStack.Size(); i++ ) {
                Console.WriteLine( $"Slot {i}: {this._callStack[i]}" );
            }
        }

        public void Initialise( string key, Codelet value ) {
            var halt = new HaltRunlet();
            var unlock = new UnlockRunlet( halt );
            var pop = new PopGlobalRunlet( this._dictionary.Get( key ), unlock );
            var init = value.Weave( pop, this._dictionary );
            Runlet currentInstruction = new LockRunlet( init );
            while (true) {
                currentInstruction = currentInstruction.ExecuteRunlet( this );
            }
        }


    }

}

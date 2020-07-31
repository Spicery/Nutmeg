using System;
using System.Collections.Generic;

namespace NutmegRunner {

    public abstract class Runlet {

        public abstract Runlet ExecuteRunlet( RuntimeEngine runtimeEngine );

        public virtual Runlet Track( Runlet runlet ) {
            return this;
        }

        public virtual void UpdateLinkNotification() {
            //  Skip
        }

    }

    public abstract class RunletWithNext : Runlet {

        protected Runlet _next;

        public Runlet Next => this._next;

        public RunletWithNext( Runlet next ) {
            while ( next is JumpRunlet j && j.Next != null ) {
                next = j.Next;
            }
            this._next = next?.Track( this );
        }

        public override void UpdateLinkNotification() {
            while ( this._next is JumpRunlet j && j.Next != null) { 
                this._next = j.Next;
            }
        }

    }

    /// <summary>
    /// The JumpRunlet is a kind of no-op that simply passes control to
    /// the next runlet. As a consequence we want to elide it wherever
    /// possible. However, the target of a JumpRunlet might be assigned
    /// _after_ it is created - unlike any other Runlet - and indeed that
    /// is the real purpose of the JumpRunlet. When the target is assigned
    /// the nodes that reference it are notified and perform the elision.
    /// </summary>
    public class JumpRunlet : RunletWithNext {

        List<Runlet> _tracked = new List<Runlet>();

        public JumpRunlet( Runlet next ) : base( next ) {
        }

        public override Runlet Track( Runlet runlet ) {
            this._tracked.Add( runlet );
            return this;
        }

        public void UpdateLink( Runlet runlet ) {
            this._next = runlet;
            foreach ( var r in this._tracked ) {
                r.UpdateLinkNotification();
            }
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            return this._next;
        }

    }

    public class HaltRunlet : Runlet {

        bool _usePrint;

        public HaltRunlet( bool usePrint ) {
            this._usePrint = usePrint;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            if ( this._usePrint ) {
                var N = runtimeEngine.ValueStackLength();
                Console.WriteLine( N != 1 ? $"There are {N} items returned" : "There is 1 item returned" );
                var N1 = N - 1;
                for ( int i = 0; i < N; i++ ) {
                    var count = i + 1;
                    var item = runtimeEngine.PeekItemOrElse( N1 - i );
                    Console.WriteLine( $"{count}: {item}" );
                    Console.Write( $"{count}: " );
                    ShowMeSystemFunction.ShowMe( item );
                    Console.WriteLine();
                }
            }
            throw new NormalExitNutmegException();
        }
    }

    public class PushIdentRunlet : RunletWithNext {

        Ident _ident;

        public PushIdentRunlet( Ident ident, Runlet next ) : base( next ) {
            this._ident = ident;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.Push( this._ident.Value );
            return this._next;
        }

    }

    public class PushSlotRunlet : RunletWithNext {
        int _slot;
        public PushSlotRunlet( int slot, Runlet next ) : base( next ) { 
            this._slot = slot;
        }
        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            //runtimeEngine.ShowFrames();
            //Console.WriteLine( $"Slot number {this._slot}" );
            runtimeEngine.PushSlot( this._slot );
            //Console.WriteLine( $"PUSHED <<{runtimeEngine.PeekOrElse()}>>" );
            return this._next;
        }
    }

    public class CallQRunlet : RunletWithNext {

        FunctionRunlet _functionRunlet;

        public CallQRunlet( FunctionRunlet fc, Runlet next ) : base( next ) {
            this._functionRunlet = fc;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.PushReturnAddress( this._next );
            return this._functionRunlet.Call( runtimeEngine );
        }

    }

    public class ReturnRunlet : Runlet {

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            return runtimeEngine.Return();
        }

    }

    public class LockRunlet : RunletWithNext {

        public LockRunlet( Runlet next ) : base( next ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.LockValueStack();
            return _next;
        }
    }

    public class UnlockRunlet : RunletWithNext {

        public UnlockRunlet( Runlet next ) : base( next ){
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.UnlockValueStack();
            return _next;
        }
    }

    public class PopGlobalRunlet : RunletWithNext {
        Ident _ident;

        public PopGlobalRunlet( Ident ident, Runlet next ) : base( next ) {
            this._ident = ident;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            this._ident.Value = runtimeEngine.Pop1();
            return _next;
        }
    }

    public class FunctionRunlet : RunletWithNext {

        private int Nargs { get; set; }
        public int Nlocals { get; set; }

        private Runlet _startCodelet = null;

        public FunctionRunlet( int nargs, int nlocals, Runlet startCodelet, Runlet next ) : base( next ) {
            this.Nargs = nargs;
            this.Nlocals = nlocals;
            this._startCodelet = startCodelet;
        }

        public Runlet Call( RuntimeEngine runtimeEngine ) {
            var nargs = runtimeEngine.CreateFrameAndCopyValueStack( this.Nlocals );
            if (nargs != this.Nargs) {
                throw
                    new NutmegException( "Mismatch in the number of arguments to the number of parameters" )
                    .Culprit( "Expected", $"{this.Nargs}" )
                    .Culprit( "Found", $"{nargs}" );
            } else {
                return this._startCodelet;
            }
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.Push( this );
            return this._next;
        }

    }

    public class CallSRunlet : RunletWithNext {

        public CallSRunlet( Runlet next ) : base( next ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            FunctionRunlet f = (FunctionRunlet)runtimeEngine.Pop();
            runtimeEngine.PushReturnAddress( this._next );
            return f.Call( runtimeEngine );
        }
    }

    public class ForkRunlet : Runlet {

        public Runlet ThenPart { get; set; }
        public Runlet ElsePart { get; set; }

        public ForkRunlet( Runlet thenPart, Runlet elsePart ) {
            this.ThenPart = thenPart.Track( this );
            this.ElsePart = elsePart.Track( this );
        }

        public override void UpdateLinkNotification() {
            while ( this.ThenPart is JumpRunlet thenj && thenj.Next != null ) {
                this.ThenPart = thenj.Next;
            }
            while ( this.ElsePart is JumpRunlet elsej && elsej.Next != null ) {
                this.ElsePart = elsej.Next;
            }
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            return runtimeEngine.PopBool() ? ThenPart : ElsePart;
        }

    }

    public class PushQRunlet : RunletWithNext {

        private object _value;

        public PushQRunlet( object value, Runlet next ) : base( next ) {
            this._value = value;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.Push( this._value );
            return this._next;
        }
    }

}

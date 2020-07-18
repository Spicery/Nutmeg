using System;
namespace NutmegRunner {

    public abstract class Runlet {
        public abstract Runlet ExecuteRunlet( RuntimeEngine runtimeEngine );
    }

    public class HaltRunlet : Runlet {
        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            throw new NormalExitNutmegException();
        }
    }

    public class PushIdentRunlet : Runlet {

        Runlet _next;
        Ident _ident;

        public PushIdentRunlet( Ident ident, Runlet next ) {
            this._ident = ident;
            this._next = next;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.Push( this._ident.Value );
            return this._next;
        }

    }

    public class PushSlotRunlet : Runlet {
        Runlet _next;
        int _slot;
        public PushSlotRunlet( int slot, Runlet next ) {
            this._slot = slot;
            this._next = next;
        }
        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            //runtimeEngine.ShowFrames();
            //Console.WriteLine( $"Slot number {this._slot}" );
            runtimeEngine.PushSlot( this._slot );
            //Console.WriteLine( $"PUSHED <<{runtimeEngine.PeekOrElse()}>>" );
            return this._next;
        }
    }

    public class CallQRunlet : Runlet {

        FunctionRunlet _functionRunlet;
        Runlet _next;

        public CallQRunlet( FunctionRunlet fc, Runlet next ) {
            this._functionRunlet = fc;
            this._next = next;
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

    public class LockRunlet : Runlet {

        Runlet _next;

        public LockRunlet( Runlet next ) {
            this._next = next;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.LockValueStack();
            return _next;
        }
    }

    public class FunctionRunlet : Runlet {

        private int Nargs { get; set; }
        public int Nlocals { get; set; }

        private Runlet _next;
        private Runlet _startCodelet = null;

        public FunctionRunlet( int nargs, int nlocals, Runlet startCodelet, Runlet next ) {
            this.Nargs = nargs;
            this.Nlocals = nlocals;
            this._startCodelet = startCodelet;
            this._next = next;
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

    public class CallSRunlet : Runlet {

        private Runlet _next;

        public CallSRunlet( Runlet next ) {
            this._next = next;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            FunctionRunlet f = (FunctionRunlet)runtimeEngine.Pop();
            runtimeEngine.PushReturnAddress( this._next );
            return f.Call( runtimeEngine );
        }
    }

    public class ForkWovenCodelet : Runlet {

        Runlet ThenPart { get; set; }
        Runlet ElsePart { get; set; }

        public ForkWovenCodelet( Runlet thenPart, Runlet elsePart ) {
            this.ThenPart = thenPart;
            this.ElsePart = elsePart;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            return runtimeEngine.PopBool() ? ThenPart : ElsePart;
        }

    }

    public class PushQRunlet : Runlet {

        private object _value;
        private Runlet _next;

        public PushQRunlet( object value, Runlet next ) {
            this._value = value;
            this._next = next;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.Push( this._value );
            return this._next;
        }
    }

}

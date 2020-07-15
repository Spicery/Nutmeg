using System;
namespace NutmegRunner {

    public abstract class Runlet {
        public abstract Runlet ExecuteRunlet( RuntimeEngine runtimeEngine );
    }

    public class HaltWovenCodelet : Runlet {
        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            throw new NormalExitNutmegException();
        }
    }

    /// <summary>
    /// This is a runtimeEngine-only class that will be synthesized on-the-fly.
    /// </summary>
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

    /// <summary>
    /// This is a runtimeEngine-only class that will be synthesized on-the-fly.
    /// </summary>
    public class ReturnRunlet : Runlet {

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            return runtimeEngine.Return();
        }

    }


    public class FunctionRunlet : Runlet {

        private int Nargs { get; set; }
        public int Nlocals { get; set; }

        private Runlet startCodelet = null;

        public FunctionRunlet( int nargs, int nlocals, Runlet startCodelet ) {
            this.Nargs = nargs;
            this.Nlocals = nlocals;
            this.startCodelet = startCodelet;
        }

        public Runlet Call( RuntimeEngine runtimeEngine ) {
            return this.startCodelet;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            throw new NutmegException( "Not Implemented" );
        }

    }

    public class ForkWovenCodelet : Runlet {

        Runlet ThenPart { get; set; }
        Runlet ElsePart { get; set; }

        public ForkWovenCodelet( Runlet thenPart, Runlet elsePart ) {
            ThenPart = thenPart;
            ElsePart = elsePart;
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

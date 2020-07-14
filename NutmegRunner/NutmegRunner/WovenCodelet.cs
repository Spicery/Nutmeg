using System;
namespace NutmegRunner {

    public abstract class WovenCodelet {
        public abstract WovenCodelet RunWovenCodelets( RuntimeEngine runtimeEngine );
    }

    public class HaltWovenCodelet : WovenCodelet {
        public override WovenCodelet RunWovenCodelets( RuntimeEngine runtimeEngine ) {
            throw new NormalExitNutmegException();
        }
    }

    /// <summary>
    /// This is a runtimeEngine-only class that will be synthesized on-the-fly.
    /// </summary>
    public class CallQWovenCodelet : WovenCodelet {

        FunctionWovenCodelet _functionCodelet;
        WovenCodelet _next;

        public CallQWovenCodelet( FunctionWovenCodelet fc, WovenCodelet next ) {
            this._functionCodelet = fc;
            this._next = next;
        }

        public override WovenCodelet RunWovenCodelets( RuntimeEngine runtimeEngine ) {
            runtimeEngine.PushReturnAddress( this._next );
            return this._functionCodelet.Call( runtimeEngine );
        }

    }

    /// <summary>
    /// This is a runtimeEngine-only class that will be synthesized on-the-fly.
    /// </summary>
    public class ReturnWovenCodelet : WovenCodelet {

        public override WovenCodelet RunWovenCodelets( RuntimeEngine runtimeEngine ) {
            return runtimeEngine.Return();
        }

    }


    public class FunctionWovenCodelet : WovenCodelet {

        private int Nargs { get; set; }
        public int Nlocals { get; set; }

        private WovenCodelet startCodelet = null;

        public FunctionWovenCodelet( int nargs, int nlocals, WovenCodelet startCodelet ) {
            this.Nargs = nargs;
            this.Nlocals = nlocals;
            this.startCodelet = startCodelet;
        }

        public WovenCodelet Call( RuntimeEngine runtimeEngine ) {
            return this.startCodelet;
        }

        public override WovenCodelet RunWovenCodelets( RuntimeEngine runtimeEngine ) {
            throw new NutmegException( "Not Implemented" );
        }

    }

    public class ForkWovenCodelet : WovenCodelet {

        WovenCodelet ThenPart { get; set; }
        WovenCodelet ElsePart { get; set; }

        public ForkWovenCodelet( WovenCodelet thenPart, WovenCodelet elsePart ) {
            ThenPart = thenPart;
            ElsePart = elsePart;
        }

        public override WovenCodelet RunWovenCodelets( RuntimeEngine runtimeEngine ) {
            return runtimeEngine.PopBool() ? ThenPart : ElsePart;
        }

    }

    public class SyscallWovenCodelet : WovenCodelet {

        private SystemFunction _systemFunction;
        private WovenCodelet _next;

        public SyscallWovenCodelet( SystemFunction s, WovenCodelet w ) {
            this._systemFunction = s;
            this._next = w;
        }

        public override WovenCodelet RunWovenCodelets( RuntimeEngine runtimeEngine ) {
            this._systemFunction( runtimeEngine );
            return this._next;
        }

    }

    public class PushQWovenCodelet : WovenCodelet {

        private object _value;
        private WovenCodelet _next;

        public PushQWovenCodelet( object value, WovenCodelet next ) {
            this._value = value;
            this._next = next;
        }

        public override WovenCodelet RunWovenCodelets( RuntimeEngine runtimeEngine ) {
            runtimeEngine.Push( this._value );
            return this._next;
        }
    }

}

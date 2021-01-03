namespace NutmegRunner.Modules.Bitwise {

    public class ANDSystemFunction : FixedAritySystemFunction {

        public ANDSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x & y );
            return this.Next;
        }

    }

    public class ORSystemFunction : FixedAritySystemFunction {

        public ORSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x | y );
            return this.Next;
        }

    }

    public class NOTSystemFunction : FixedAritySystemFunction {

        public NOTSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( ~x );
            return this.Next;
        }

    }

    public class LeftShiftSystemFunction : FixedAritySystemFunction {

        public LeftShiftSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x << (int)y );
            return this.Next;
        }

    }

    public class RightShiftSystemFunction : FixedAritySystemFunction {

        public RightShiftSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x >> (int)y );
            return this.Next;
        }

    }

    public class XORSystemFunction : FixedAritySystemFunction {

        public XORSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x ^ y );
            return this.Next;
        }

    }

    public class BitwiseModule : SystemFunctionsModule{
        public override void AddAll() {
            this.Add( "AND", r => new ANDSystemFunction( r ) );
            this.Add( "OR", r => new ORSystemFunction( r ) );
            this.Add( "XOR", r => new XORSystemFunction( r ) );
            this.Add( "NOT", r => new NOTSystemFunction( r ) );
            this.Add( "LSHIFT", r => new LeftShiftSystemFunction( r ) );
            this.Add( "RSHIFT", r => new RightShiftSystemFunction( r ) );
        }
    }

}

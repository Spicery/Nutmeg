using System;
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
            Add( "AND", r => new ANDSystemFunction( r ) );
            Add( "OR", r => new ANDSystemFunction( r ) );
            Add( "XOR", r => new ANDSystemFunction( r ) );
            Add( "NOT", r => new ANDSystemFunction( r ) );
        }
    }

}

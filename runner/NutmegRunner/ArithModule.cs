namespace NutmegRunner.Modules.Arith {

    public class AddSystemFunction : FixedAritySystemFunction {

        public AddSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x + y );
            return this.Next;
        }

    }

    public class MulSystemFunction : FixedAritySystemFunction {

        public MulSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x * y );
            return this.Next;
        }

    }

    public class SumSystemFunction : VariadicSystemFunction {

        public SumSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long n = 0;
            while (runtimeEngine.TryPop( out var d )) {
                n += (long)d;
            }
            runtimeEngine.PushValue( n );
            return this.Next;
        }

    }

    public class SubtractSystemFunction : FixedAritySystemFunction {

        public SubtractSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x - y );
            return this.Next;
        }

    }

    public class ArithModule : SystemFunctionsModule {
        public override void AddAll() {
            Add( "+", r => new AddSystemFunction( r ), "add" );
            Add( "*", r => new MulSystemFunction( r ), "mul" );
            Add( "-", r => new SubtractSystemFunction( r ), "sub" );
            Add( "sum", r => new SumSystemFunction( r ) );
        }
    }
}

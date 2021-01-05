namespace NutmegRunner.Modules.Arith {

    public class MaxSystemFunction : VariadicSystemFunction {

        public MaxSystemFunction( Runlet next ) : base( next ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int N = runtimeEngine.ValueStackLength();
            switch (N) {
                case 0:
                    throw new NutmegException( "Cannot take max of no arguments" ).Hint( "Full arithmetic not yet implemented" );
                case 1:
                    //  No action needed!
                    break;
                default:
                    long sofar = (long)runtimeEngine.GetItem( 0 );    // initialise it.
                    for ( int i = 1; i < N; i++ ) {
                        var n = (long)runtimeEngine.GetItem( i );
                        if (n > sofar) sofar = n;
                    }
                    runtimeEngine.ClearValueStack();
                    runtimeEngine.PushValue( sofar );
                    break;
            }
            return this.Next;
        }

    }

    public class MinSystemFunction : VariadicSystemFunction {

        public MinSystemFunction( Runlet next ) : base( next ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int N = runtimeEngine.ValueStackLength();
            switch (N) {
                case 0:
                    throw new NutmegException( "Cannot take min of no arguments" ).Hint( "Full arithmetic not yet implemented" );
                case 1:
                    //  No action needed!
                    break;
                default:
                    long sofar = (long)runtimeEngine.GetItem( 0 );    // initialise it.
                    for (int i = 1; i < N; i++) {
                        var n = (long)runtimeEngine.GetItem( i );
                        if (n < sofar) sofar = n;
                    }
                    runtimeEngine.ClearValueStack();
                    runtimeEngine.PushValue( sofar );
                    break;
            }
            return this.Next;
        }

    }

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

    public class ProductSystemFunction : VariadicSystemFunction {

        public ProductSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long n = 1;
            while (runtimeEngine.TryPop( out var d )) {
                n *= (long)d;
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

    public class IntegerDivisionSystemFunction : FixedAritySystemFunction {

        public IntegerDivisionSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x / y );
            return this.Next;
        }

    }

    public class IntegerRemainderSystemFunction : FixedAritySystemFunction {

        public IntegerRemainderSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x % y );
            return this.Next;
        }

    }

    public class ArithModule : SystemFunctionsModule {
        public override void AddAll() {
            Add( "+", r => new AddSystemFunction( r ), "add" );
            Add( "*", r => new MulSystemFunction( r ), "mul" );
            Add( "-", r => new SubtractSystemFunction( r ), "sub" );
            Add( "sum", r => new SumSystemFunction( r ) );
            Add( "product", r => new ProductSystemFunction( r ) );
            Add( "max", r => new MaxSystemFunction( r ) );
            Add( "min", r => new MinSystemFunction( r ) );
            Add( "quot", r => new IntegerDivisionSystemFunction( r ) );
            Add( "rem", r => new IntegerRemainderSystemFunction( r ) );
        }
    }
}

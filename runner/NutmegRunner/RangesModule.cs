namespace NutmegRunner.Modules.Ranges {

    public class HalfOpenRangeListSystemFunction : FixedAritySystemFunction {

        public HalfOpenRangeListSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            var list = new HalfOpenRangeList( x, y );
            runtimeEngine.PushValue( list );
            return this.Next;
        }

    }

    public class ClosedRangeListSystemFunction : FixedAritySystemFunction {

        public ClosedRangeListSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            var list = new HalfOpenRangeList( x, y + 1 );
            runtimeEngine.PushValue( list );
            return this.Next;
        }

    }

    public class HalfOpenRangeSystemFunction : FixedAritySystemFunction {

        public HalfOpenRangeSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            for (long i = x; i < y; i++) {
                runtimeEngine.PushValue( i );
            }
            return this.Next;
        }

    }

    public class ClosedRangeSystemFunction : FixedAritySystemFunction {

        public ClosedRangeSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            for (long i = x; i <= y; i++) {
                runtimeEngine.PushValue( i );
            }
            return this.Next;
        }

    }


    public class RangesModule : SystemFunctionsModule {
        public override void AddAll() {
            Add( "..<", r => new HalfOpenRangeSystemFunction( r ), "halfOpenRange" );
            Add( "...", r => new ClosedRangeSystemFunction( r ), "closedRange" );
            Add( "[x..<y]", r => new HalfOpenRangeListSystemFunction( r ), "halfOpenRangeList" );
            Add( "[x...y]", r => new ClosedRangeListSystemFunction( r ), "closedRangeList" );
        }
    }

}

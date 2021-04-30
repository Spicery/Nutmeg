namespace NutmegRunner.Modules.Ranges {

    public class RangeSystemFunction : VariadicSystemFunction {

        public RangeSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int n = runtimeEngine.NArgs0();
            long step = 1;
            switch (n) {
                case 2:
                    break;
                case 3:
                    step = (long)runtimeEngine.PopValue();
                    break;
                default:
                    throw new NutmegException( "Wrong number of arguments for range" ).Culprit( "Expecting", "2 or 3 arguments" ).Culprit( "Received", $"{n}" );
            }
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            var list = HalfOpenRangeList.New( x, y, step );
            runtimeEngine.PushValue( list );
            return this.Next;
        }

    }

    public class HalfOpenRangeListSystemFunction : FixedAritySystemFunction {

        public HalfOpenRangeListSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            var list = HalfOpenRangeList.New( x, y );
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
            var list = HalfOpenRangeList.New( x, y + 1 );
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
            Add( "range", r => new RangeSystemFunction( r ) );
            Add( "..<", r => new HalfOpenRangeSystemFunction( r ), "halfOpenRange" );
            Add( "...", r => new ClosedRangeSystemFunction( r ), "closedRange" );
            Add( "[x..<y]", r => new HalfOpenRangeListSystemFunction( r ), "halfOpenRangeList" );
            Add( "[x...y]", r => new ClosedRangeListSystemFunction( r ), "closedRangeList" );
        }
    }

}

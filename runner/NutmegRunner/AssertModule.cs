using System;

namespace NutmegRunner.Modules.Assert {

    public class AssertTrueSystemFunction : FixedAritySystemFunction {

        public AssertTrueSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 3;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var position = runtimeEngine.PopValue();
            var unit = runtimeEngine.PopValue();
            switch (runtimeEngine.PopValue()) {
                case Boolean b:
                    if (b) return this.Next;
                    break;
                default:
                    break;
            }
            throw new AssertionFailureException( "assert failed", unit, position );
        }

    }

    public class AssertEqualsSystemFunction : FixedAritySystemFunction {

        public AssertEqualsSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 4;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var position = runtimeEngine.PopValue();
            var unit = runtimeEngine.PopValue();
            var y = runtimeEngine.PopValue();
            var x = runtimeEngine.PopValue();
            if (EqualsSystemFunction.GeneralEquals( x, y ) ) {
                return this.Next;
            } else {
                throw new AssertionFailureException( "assert equal failed", unit, position )
                    .Culprit( "Left Value", ShowMeSystemFunction.ShowMeAsString( x ) )
                    .Culprit( "Right Value", ShowMeSystemFunction.ShowMeAsString( y ) );
            }
        }

    }

    public class AssertNotEqualsSystemFunction : FixedAritySystemFunction {

        public AssertNotEqualsSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 4;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var position = runtimeEngine.PopValue();
            var unit = runtimeEngine.PopValue();
            var y = runtimeEngine.PopValue();
            var x = runtimeEngine.PopValue();
            if (!(x?.Equals( y ) ?? y == null)) {
                return this.Next;
            } else {
                throw new AssertionFailureException( "assert not equals failed", unit, position )
                    .Culprit( "Left Value", $"{x}" )
                    .Culprit( "Right Value", $"{y}" );
            }
        }

    }

    public class AssertModule : SystemFunctionsModule {
        public override void AddAll() {
            Add( "assertTrue", r => new AssertTrueSystemFunction( r ) );
            Add( "assertEquals", r => new AssertEqualsSystemFunction( r ) );
            Add( "assertNotEquals", r => new AssertNotEqualsSystemFunction( r ) );
        }
    }
}

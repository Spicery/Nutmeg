using System.Collections.Generic;

namespace NutmegRunner.Modules.Seqs {


    public class Length : FixedAritySystemFunction {

        public Length( Runlet next ) : base( next ) {
        }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var obj = runtimeEngine.PopValue();
            switch (obj) {
                case string x:
                    runtimeEngine.PushValue( (long)x.Length );
                    break;
                case ICollection<object> list:
                    runtimeEngine.PushValue( (long)list.Count );
                    break;
                default:
                    //  Ought to be extended to support methods in the future.
                    throw new NutmegException( "Unexpected type of argument for length" ).Culprit( "Argument", $"{obj}" );
            }
            return this.Next;
        }

    }

    public class Get : VariadicSystemFunction {

        public Get( Runlet next ) : base( next ) {
        }

        private static void GeneralPush( RuntimeEngine runtimeEngine, object obj, int pos ) {
            switch (obj) {
                case string s:
                    runtimeEngine.PushValue( s[pos] );
                    break;
                case IList<object> list:
                    runtimeEngine.PushValue( list[pos] );
                    break;
                default:
                    //  Ought to be extended to support methods in the future.
                    throw new NutmegException( "Invalid argument for get" ).Culprit( "Argument", $"{pos}" );
            }
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int N = runtimeEngine.NArgs0();
            switch (N) {
                case 0:
                    throw new NutmegException( "No arguments for get" ).Hint( "At least 1 is needed" );
                case 1:
                    runtimeEngine.ClearValueStack();
                    break;
                case 2: {
                        //  This is the common case that merits optimisation.
                        object pos = runtimeEngine.PopValue();
                        object x = runtimeEngine.PopValue();
                        GeneralPush( runtimeEngine, x, (int)(long)pos );
                    }
                    break;
                default: {
                        var args = runtimeEngine.PopManyToList( N - 1 );
                        object x = runtimeEngine.PopValue();
                        foreach (var arg in args) {
                            GeneralPush( runtimeEngine, x, (int)(long)arg );
                        }
                    }
                    break;

            }
            return this.Next;
        }

    }

    public class SeqsModule : SystemFunctionsModule {
        public override void AddAll() {
            Add( "length", r => new Length( r ) );
            Add( "get", r => new Get( r ) );
        }
    }

}

using System;
using System.Collections.Generic;

namespace NutmegRunner {

    public abstract class SystemFunction : Runlet, ICallable {

        public SystemFunction( Runlet next ) {
            this.Next = next;
        }

        public Runlet Next { get; set; }

        public Runlet Call( RuntimeEngine runtimeEngine, Runlet next, bool alt ) {
            this.ExecuteRunlet( runtimeEngine );
            return next;
        }

        public override IEnumerable<Runlet> Neighbors() {
            return new List<Runlet> { Next };
        }        

    }

    public abstract class FixedAritySystemFunction : SystemFunction {

        public FixedAritySystemFunction( Runlet next ) : base( next ) {
        }

        public abstract int Nargs { get; }

    }


    public abstract class VariadicSystemFunction : SystemFunction {

        public VariadicSystemFunction( Runlet next ) : base( next ) {
        }

    }

    public class PrintlnSystemFunction : VariadicSystemFunction {

        public PrintlnSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var sep = " ";
            var first = true;
            foreach (var item in runtimeEngine.PopAll()) {
                if ( ! first ) {
                    Console.Write( sep );
                }
                Console.Write( $"{item}" );
                first = false;
            }
            Console.WriteLine();
            return this.Next;
        }

    }

    public class ShowMeSystemFunction : VariadicSystemFunction {

        public ShowMeSystemFunction( Runlet next ) : base( next ) { }

        static public void ShowMe( HalfOpenRangeList horl ) {
            Console.Write( $"[{horl.Low}..<{horl.High}]" );
        }

        static public void ShowMe( IReadOnlyList<object> list ) {
            Console.Write( "[" );
            var first = true;
            foreach (var i in list) {
                if (!first) {
                    Console.Write( ", " );
                }
                ShowMe( i );
                first = false;
            }
            Console.Write( "]" );
        }

        static public void ShowMe( string s ) {
            //  TODO - escape quotes etc
            Console.Write( $"\"{s}\"" );
        }

        static public void ShowMe( object item ) {
            Console.Write( $"{item}" );
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var sep = " ";
            var first = true;
            foreach (var item in runtimeEngine.PopAll()) {
                if (!first) {
                    Console.Write( sep );
                }
                ShowMe( (dynamic)item );
                first = false;
            }
            Console.WriteLine();
            return this.Next;
        }

    }

    public class StreamSystemFunction : FixedAritySystemFunction {

        public StreamSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            object x = runtimeEngine.PopValue();
            switch (x) {
                case IEnumerable<object> e:
                    runtimeEngine.PushValue( e.GetEnumerator() );
                    break;
                default:
                    throw new NutmegException( $"Cannot stream this object: {x}" );
            }
            return this.Next;
        }

    }

    public class ListSystemFunction : VariadicSystemFunction {

        public ListSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.PushValue( runtimeEngine.PopAll( immutable: true ) );
            return this.Next;
        }
    }

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
            while ( runtimeEngine.TryPop( out var d ) ) {
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

    public class LTESystemFunction : FixedAritySystemFunction {

        public LTESystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x <= y );
            return this.Next;
        }

    }

    public class LTSystemFunction : FixedAritySystemFunction {

        public LTSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x < y );
            return this.Next;
        }

    }

    public class GTESystemFunction : FixedAritySystemFunction {

        public GTESystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x >= y );
            return this.Next;
        }

    }

    public class GTSystemFunction : FixedAritySystemFunction {

        public GTSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.PopValue();
            long x = (long)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x > y );
            return this.Next;
        }

    }

    public class AssertSystemFunction : FixedAritySystemFunction {

        public AssertSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            switch ( runtimeEngine.PopValue() ) {
                case Boolean b:
                    if (b) return this.Next;
                    break;
                default:
                    break;
            }
            throw new NutmegTestFailException();
        }

    }

    public delegate SystemFunction SystemFunctionMaker( Runlet next );

    public class LookupTableBuilder {

        public Dictionary<string, SystemFunctionMaker> Table { get; } = new Dictionary<string, SystemFunctionMaker>();

        public LookupTableBuilder Add( string sysname, SystemFunctionMaker f, params string[] synonyms ) {
            this.Table.Add( sysname, f );
            foreach ( var name in synonyms ) {
                this.Table.Add( name, f );
            }
            return this;
        }

    }



    public class NutmegSystem {

        static readonly Dictionary<string, SystemFunctionMaker> SYSTEM_FUNCTION_TABLE =
            new LookupTableBuilder()
            .Add( "println", r => new PrintlnSystemFunction( r ) )
            .Add( "showMe", r => new ShowMeSystemFunction( r ) )
            .Add( "..<", r => new HalfOpenRangeSystemFunction( r ), "halfOpenRange" )
            .Add( "...", r => new ClosedRangeSystemFunction( r ), "closedRange" )
            .Add( "[x..<y]", r => new HalfOpenRangeListSystemFunction( r ), "halfOpenRangeList" )
            .Add( "[x...y]", r => new ClosedRangeListSystemFunction( r ), "closedRangeList" )
            .Add( "+", r => new AddSystemFunction( r ), "add" )
            .Add( "*", r => new MulSystemFunction( r ), "mul" )
            .Add( "-", r => new SubtractSystemFunction( r ), "sub" )
            .Add( "sum", r => new SumSystemFunction( r ) )
            .Add( "<=", r => new LTESystemFunction( r ), "lessThanOrEqualTo" )
            .Add( "<", r => new LTSystemFunction( r ), "lessThan" )
            .Add( ">=", r => new GTESystemFunction( r ), "greaterThanOrEqualTo" )
            .Add( ">", r => new GTSystemFunction( r ), "greaterThan" )
            .Add( "newImmutableList", r => new ListSystemFunction( r ) )
            .Add( "assert", r => new AssertSystemFunction( r ) )
            .Table;

        public static SystemFunctionMaker Find( string name ) {
            if (SYSTEM_FUNCTION_TABLE.TryGetValue( name, out var value )) {
                return value;
            } else {
                throw new NutmegException( "Cannot resolve the following system function" )
                    .Culprit( "Identifier", name );
            }
        }

    }

}

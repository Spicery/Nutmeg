using System;
using System.Collections.Generic;

namespace NutmegRunner {

    public abstract class SystemFunction : Runlet {

        public SystemFunction( Runlet next ) {
            this.Next = next;
        }

        public Runlet Next { get; set; }
      
    }

    public class PrintlnSystemFunction : SystemFunction {

        public PrintlnSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            Console.WriteLine( $"{runtimeEngine.Pop()}" );
            return this.Next;
        }

    }

    public class HalfOpenRangeSystemFunction : SystemFunction {

        public HalfOpenRangeSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.Pop();
            long x = (long)runtimeEngine.Pop();
            for (long i = x; i < y; i++) {
                runtimeEngine.Push( i );
            }
            runtimeEngine.UnlockValueStack();
            return this.Next;
        }

    }

    public class ClosedRangeSystemFunction : SystemFunction {

        public ClosedRangeSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.Pop();
            long x = (long)runtimeEngine.Pop();
            for (long i = x; i <= y; i++) {
                runtimeEngine.Push( i );
            }
            runtimeEngine.UnlockValueStack();
            return this.Next;
        }

    }

    public class AddSystemFunction : SystemFunction {

        public AddSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.Pop();
            long x = (long)runtimeEngine.Pop();
            runtimeEngine.Push( x + y );
            runtimeEngine.UnlockValueStack();
            return this.Next;
        }

    }

    public class SumSystemFunction : SystemFunction {

        public SumSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long n = 0;
            while ( runtimeEngine.TryPop( out var d ) ) {
                n += (long)d;
            }
            runtimeEngine.Push( n );
            runtimeEngine.UnlockValueStack();
            return this.Next;
        }

    }

    public class SubtractSystemFunction : SystemFunction {

        public SubtractSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.Pop();
            long x = (long)runtimeEngine.Pop();
            runtimeEngine.Push( x - y );
            runtimeEngine.UnlockValueStack();
            return this.Next;
        }

    }

    public class LTESystemFunction : SystemFunction {

        public LTESystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long y = (long)runtimeEngine.Pop();
            long x = (long)runtimeEngine.Pop();
            runtimeEngine.Push( x <= y );
            runtimeEngine.UnlockValueStack();
            return this.Next;
        }

    }

    public delegate SystemFunction SystemFunctionMaker( Runlet next );

    public class LookupTableBuilder {

        public Dictionary<string, SystemFunctionMaker> Table { get; } = new Dictionary<string, SystemFunctionMaker>();

        public LookupTableBuilder Add( string sysname, SystemFunctionMaker f ) {
            this.Table.Add( sysname, f );
            return this;
        }

    }

    public class NutmegSystem {

        static readonly Dictionary<string, SystemFunctionMaker> SYSTEM_FUNCTION_TABLE =
            new LookupTableBuilder()
            .Add( "println", r => new PrintlnSystemFunction( r ) )
            .Add( "..<", r => new HalfOpenRangeSystemFunction( r ) )
            .Add( "...", r => new ClosedRangeSystemFunction( r ) )
            .Add( "+", r => new AddSystemFunction( r ) )
            .Add( "sum", r => new SumSystemFunction( r ) )
            .Add( "-", r => new SubtractSystemFunction( r ) )
            .Add( "<=", r => new LTESystemFunction( r ) )
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

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

    public class AddSystemFunction : SystemFunction {

        public AddSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int y = (int)runtimeEngine.Pop();
            int x = (int)runtimeEngine.Pop();
            runtimeEngine.Push( x + y );
            return this.Next;
        }

    }

    public class SubtractSystemFunction : SystemFunction {

        public SubtractSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int y = (int)runtimeEngine.Pop();
            int x = (int)runtimeEngine.Pop();
            runtimeEngine.Push( x - y );
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

    public class System {

        static readonly Dictionary<string, SystemFunctionMaker> SYSTEM_FUNCTION_TABLE =
            new LookupTableBuilder()
            .Add( "println", r => new PrintlnSystemFunction( r ) )
            .Add( "+", r => new AddSystemFunction( r ) )
            .Add( "-", r => new SubtractSystemFunction( r ) )
            .Table;

        public static SystemFunctionMaker Find( string name ) {
            return SYSTEM_FUNCTION_TABLE.TryGetValue( name, out var value ) ? value : null;
        }

    }

}

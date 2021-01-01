using System;
using System.Collections;
using System.Collections.Generic;
using NutmegRunner.Modules.Arith;
using NutmegRunner.Modules.Assert;
using NutmegRunner.Modules.Characters;
using NutmegRunner.Modules.Ranges;
using NutmegRunner.Modules.Seqs;
using NutmegRunner.Modules.Strings;

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

    public abstract class SystemFunctionsModule {
        readonly LookupTableBuilder builder = new LookupTableBuilder();

        public abstract void AddAll();

        public void Add( string name, SystemFunctionMaker m, params string[] synonyms ) {
            this.builder.Add( name, m, synonyms );
        }

        public LookupTableBuilder LookupTableBuilder() {
            this.AddAll();
            return this.builder;
        }
    }

    public delegate SystemFunction SystemFunctionMaker( Runlet next );

    public class LookupTableBuilder {

        public Dictionary<string, SystemFunctionMaker> Table { get; } = new Dictionary<string, SystemFunctionMaker>();

        public LookupTableBuilder Add( string sysname, SystemFunctionMaker f, params string[] synonyms ) {
            this.Table.Add( sysname, f );
            foreach (var name in synonyms) {
                this.Table.Add( name, f );
            }
            return this;
        }

        public LookupTableBuilder Add( SystemFunctionsModule module ) {
            LookupTableBuilder b = module.LookupTableBuilder();
            foreach ( var item in b.Table ) {
                this.Add( item.Key, item.Value );
            }
            return this;
        }

    }


    ////////////////////////////////////////////////////////////////

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
                if (!first) {
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

    class StringToEnumerator : IEnumerator<object> {

        CharEnumerator _src;

        public StringToEnumerator( CharEnumerator src ) {
            this._src = src;
        }

        public object Current => this._src.Current;

        public void Dispose() {
            this._src.Dispose();
        }

        public bool MoveNext() => this._src.MoveNext();

        public void Reset() => this._src.Reset();

    }

    public class StreamSystemFunction : FixedAritySystemFunction {

        public StreamSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 1;

        public static IEnumerator<object> ToStream( object x ) {
            switch (x) {
                case IEnumerable<object> e:
                    return e.GetEnumerator();
                case string s:
                    return new StringToEnumerator( s.GetEnumerator() );
                default:
                    throw new NutmegException( $"Cannot stream this object: {x}" );
            }
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.PushValue( ToStream( runtimeEngine.PopValue() ) );
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

    public class EqualsSystemFunction : FixedAritySystemFunction {

        public EqualsSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var y = runtimeEngine.PopValue();
            var x = runtimeEngine.PopValue();
            runtimeEngine.PushValue( x?.Equals( y ) ?? y == null );
            return this.Next;
        }
    }

    public class NotEqualsSystemFunction : FixedAritySystemFunction {

        public NotEqualsSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var y = runtimeEngine.PopValue();
            var x = runtimeEngine.PopValue();
            runtimeEngine.PushValue( !(x?.Equals( y ) ?? y == null) );
            return this.Next;
        }
    }

    public class NotSystemFunction : FixedAritySystemFunction {

        public NotSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.Update( 0, x => !(bool)x );
            return this.Next;
        }

    }

    public class CountArgumentsSystemFunction : VariadicSystemFunction {
        public CountArgumentsSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int n = runtimeEngine.ValueStackLength();
            runtimeEngine.ClearValueStack();
            runtimeEngine.PushValue( (long)n );
            return this.Next;
        }
    }

    public class NutmegSystem {

        static readonly Dictionary<string, SystemFunctionMaker> SYSTEM_FUNCTION_TABLE =
            new LookupTableBuilder()
            .Add( "println", r => new PrintlnSystemFunction( r ) )
            .Add( "showMe", r => new ShowMeSystemFunction( r ) )

            .Add( "==", r => new EqualsSystemFunction( r ) )
            .Add( "!=", r => new NotEqualsSystemFunction( r ) )
            .Add( "<=", r => new LTESystemFunction( r ), "lessThanOrEqualTo" )
            .Add( "<", r => new LTSystemFunction( r ), "lessThan" )
            .Add( ">=", r => new GTESystemFunction( r ), "greaterThanOrEqualTo" )
            .Add( ">", r => new GTSystemFunction( r ), "greaterThan" )
            .Add( "not", r => new NotSystemFunction( r ) )
            .Add( "newImmutableList", r => new ListSystemFunction( r ) )
            .Add( "countArguments", r => new CountArgumentsSystemFunction( r ) )
            .Add( new ArithModule() )
            .Add( new RangesModule() )
            .Add( new AssertModule() )
            .Add( new StringsModule() )
            .Add( new CharactersModule() )
            .Add( new SeqsModule() )
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

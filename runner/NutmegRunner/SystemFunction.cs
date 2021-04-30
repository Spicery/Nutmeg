using System;
using System.Collections.Generic;
using System.IO;
using NutmegRunner.Modules.Arith;
using NutmegRunner.Modules.Assert;
using NutmegRunner.Modules.Bitwise;
using NutmegRunner.Modules.Characters;
using NutmegRunner.Modules.Ranges;
using NutmegRunner.Modules.Refs;
using NutmegRunner.Modules.Seqs;
using NutmegRunner.Modules.Strings;

namespace NutmegRunner {

    public abstract class SystemFunction : Runlet, ICallable {

        public SystemFunction( Runlet next ) {
            this.Next = next;
        }

        public Runlet Next { get; set; }

        public Runlet Call( RuntimeEngine runtimeEngine, Runlet next, bool alt ) {
            if (alt) {
                this.AltExecuteRunlet( runtimeEngine );
            } else {
                this.ExecuteRunlet( runtimeEngine );
            }
            return next;
        }

        public virtual Runlet UpdateRunlet( RuntimeEngine runtimeEngine ) {
            throw new NutmegException( "No updater defined" );
        }

        public virtual Runlet AltUpdateRunlet( RuntimeEngine runtimeEngine ) {
            throw new NutmegException( "No updater defined" );
        }

        public virtual Runlet Update( RuntimeEngine runtimeEngine, Runlet next, bool alt ) {
            if (alt) {
                this.AltUpdateRunlet( runtimeEngine );
            } else {
                this.UpdateRunlet( runtimeEngine );
            }
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

    public interface IFixedAritySystemUpdater {
        public ( int, int ) UNargs { get; }
    }

    public abstract class FixedAritySystemFunction : SystemFunction {

        public FixedAritySystemFunction( Runlet next ) : base( next ) {
        }

        public abstract int Nargs { get; }

    }

    public abstract class UnarySystemFunction : FixedAritySystemFunction {
        public UnarySystemFunction( Runlet next ) : base( next ) {
        }
        public override int Nargs => 1;
        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.ApplyUnaryFunction( this.Apply );
            return this.Next;
        }
        public abstract object Apply( object x );
    }

    public abstract class UnaryToVoidSystemFunction : FixedAritySystemFunction {
        public UnaryToVoidSystemFunction( Runlet next ) : base( next ) {
        }
        public override int Nargs => 1;
        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.ApplyUnaryToVoidFunction( this.Apply );
            return this.Next;
        }
        public abstract void Apply( object x );
    }

    public abstract class VariadicSystemFunction : SystemFunction {

        public VariadicSystemFunction( Runlet next ) : base( next ) {
        }

    }

    public class PrintlnSystemFunction : VariadicSystemFunction {

        public PrintlnSystemFunction( Runlet next ) : base( next ) { }

        private static void GeneralPrint( object item ) {
            switch ( item ) {
                case ICollection<object> list:
                    Console.Write( "[" );
                    var sep = ",";
                    var first = true;
                    foreach ( var i in list ) {
                        if (!first) {
                            Console.Write( sep );
                        }
                        GeneralPrint( i );
                        first = false;
                    }
                    Console.Write( "]" );
                    break;
                default:
                    Console.Write( $"{item}" );
                    break;
            }
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var sep = " ";
            var first = true;
            var nargs = runtimeEngine.NArgs0();
            foreach (var item in runtimeEngine.PopManyToList( nargs ) ) {
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

        static public string ShowMeAsString( object item ) {
            var to_string = new StringWriter();
            ShowMe( to_string, item );
            return to_string.ToString();
        }

        static public void ShowMe( TextWriter t, object item ) {
            switch (item) {
                case string s:
                    t.Write( $"\"{s}\"" );
                    break;
                case HalfOpenRangeList horl:
                    if (horl.Step == 1) {
                        t.Write( $"[{horl.Start}..<{horl.End}]" );
                    } else {
                        t.Write( $"range({horl.Start}, {horl.End}, {horl.Step})" );
                    }
                    break;
                case ICollection<object> collection:
                    t.Write( "[" );
                    var sep = ",";
                    var first = true;
                    foreach (var i in collection) {
                        if (!first) {
                            t.Write( sep );
                        }
                        ShowMe( t, i );
                        first = false;
                    }
                    t.Write( "]" );
                    break;
                default:
                    t.Write( $"{item}" );
                    break;
            }
        }

        public ShowMeSystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var sep = " ";
            var first = true;
            var n = runtimeEngine.NArgs0();
            foreach (var item in runtimeEngine.PopManyToList( n ) ) {
                if (!first) {
                    Console.Write( sep );
                }
                ShowMe( Console.Out, item );
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
            var n = runtimeEngine.NArgs0();
            runtimeEngine.PushValue( runtimeEngine.PopManyToImmutableList( n ) );
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

        public static bool GeneralEquals( object x, object y ) {
            switch ( x ) {
                case null:
                    return y == null;
                case IList<object> xlist:
                    if (y is IList<object> ylist) {
                        if (xlist.Count != ylist.Count) return false;
                        for ( int i = 0; i < xlist.Count; i++ ) {
                            var xmember = xlist[i];
                            var ymember = ylist[i];
                            if (!GeneralEquals( xmember, ymember )) return false;
                        }
                        return true;
                    } else {
                        return false;
                    }
                default:
                    return x.Equals( y );
            }
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var y = runtimeEngine.PopValue();
            var x = runtimeEngine.PopValue();
            runtimeEngine.PushValue( GeneralEquals( x, y ) );
            return this.Next;
        }

    }

    public class NotEqualsSystemFunction : FixedAritySystemFunction {

        public NotEqualsSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var y = runtimeEngine.PopValue();
            var x = runtimeEngine.PopValue();
            runtimeEngine.PushValue( !EqualsSystemFunction.GeneralEquals( x, y ) );
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
            int n = runtimeEngine.NArgs0();
            runtimeEngine.DropValues( n );
            runtimeEngine.PushValue( (long)n );
            return this.Next;
        }
    }

    public class DupSystemFunction : FixedAritySystemFunction {
        public DupSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int ncopies = (int)(long)runtimeEngine.PopValue();
            object obj = runtimeEngine.PopValue();
            for ( int i = 0; i < ncopies; i++ ) {
                runtimeEngine.PushValue( obj );
            }
            return this.Next;
        }
    }

    public class PartApplySystemFunction : VariadicSystemFunction {
        public PartApplySystemFunction( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int N = runtimeEngine.NArgs0();
            var args = runtimeEngine.PopManyToList( N - 1 );
            ICallable fn = (ICallable)runtimeEngine.PopValue();
            runtimeEngine.PushValue( new PartialApplication( fn, args, null ) );
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
            .Add( "dup", r => new DupSystemFunction( r ) )
            .Add( "partApply", r => new PartApplySystemFunction( r ) )
            .Add( new ArithModule() )
            .Add( new RangesModule() )
            .Add( new AssertModule() )
            .Add( new StringsModule() )
            .Add( new CharactersModule() )
            .Add( new SeqsModule() )
            .Add( new BitwiseModule() )
            .Add( new RefsModule() )
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

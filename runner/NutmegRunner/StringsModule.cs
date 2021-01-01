using System.Text;

namespace NutmegRunner.Modules.Strings {

    public class StringLength : FixedAritySystemFunction {

        public StringLength( Runlet next ) : base( next ) {
        }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            string x = (string)runtimeEngine.PopValue();
            runtimeEngine.PushValue( (long)x.Length );
            return this.Next;
        }

    }

    public class StringGet : VariadicSystemFunction {

        public StringGet( Runlet next ) : base( next ) {
        }

        private static void GeneralPush( RuntimeEngine runtimeEngine, string x, object pos ) {
            switch (pos) {
                case long index:
                    runtimeEngine.PushValue( x[(int)index] );
                    break;
                default:
                    throw new NutmegException( "Invalid argument for get" ).Hint( "Integer position needed" ).Culprit( "Argument", $"{pos}" );
            }
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int N = runtimeEngine.ValueStackLength();
            switch (N) {
                case 0:
                    throw new NutmegException( "No arguments for get" ).Hint( "At least 1 is needed" );
                case 1:
                    runtimeEngine.ClearValueStack();
                    break;
                case 2: {
                        //  This is the common case that merits optimisation.
                        object pos = runtimeEngine.PopValue();
                        string x = (string)runtimeEngine.PopValue();
                        GeneralPush( runtimeEngine, x, pos );
                    }
                    break;
                default: {
                        var args = runtimeEngine.PopMany( N - 1 );
                        string x = (string)runtimeEngine.PopValue1();
                        foreach ( var arg in args ) {
                            GeneralPush( runtimeEngine, x, arg );
                        }
                    }
                    break;

            }
            return this.Next;
        }

    }

    public class StringStartsWith : FixedAritySystemFunction {

        public StringStartsWith( Runlet next ) : base( next ) {
        }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var putativePrefix = (string)runtimeEngine.PopValue();
            string subject = (string)runtimeEngine.PopValue();
            runtimeEngine.PushValue( subject.StartsWith( putativePrefix ) );
            return this.Next;
        }

    }

    public class StringEndsWith : FixedAritySystemFunction {

        public StringEndsWith( Runlet next ) : base( next ) {
        }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var putativeSuffix = (string)runtimeEngine.PopValue();
            string subject = (string)runtimeEngine.PopValue();
            runtimeEngine.PushValue( subject.EndsWith( putativeSuffix ) );
            return this.Next;
        }

    }

    public class StringContains : FixedAritySystemFunction {

        public StringContains( Runlet next ) : base( next ) {
        }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var needle = runtimeEngine.PopValue();
            string haystack = (string)runtimeEngine.PopValue();
            if (needle is string needle_string) {
                runtimeEngine.PushValue( haystack.Contains( needle_string ) );
            } else if (needle is char needle_char) {
                runtimeEngine.PushValue( haystack.Contains( needle_char ) );
            }
            return this.Next;
        }

    }



    /// <summary>
    /// Once we add optional arguments we will allow TrimLeft and TrimRight as options.
    /// </summary>
    public class StringTrim : FixedAritySystemFunction {

        public StringTrim( Runlet next ) : base( next ) {
        }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            string subject = (string)runtimeEngine.PopValue();
            runtimeEngine.PushValue( subject.Trim() );
            return this.Next;
        }

    }

    public class StringIndexOf : FixedAritySystemFunction {

        public StringIndexOf( Runlet next ) : base( next ) {
        }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var target = runtimeEngine.PopValue();
            string subject = (string)runtimeEngine.PopValue();

            switch (target) {
                case string s:
                    var n0 = subject.IndexOf( s );
                    if (n0 < 0) {
                        throw new NutmegException( "Cannot find index" ).Culprit( "Subject", subject ).Culprit( "Argument", s );
                    } else {
                        runtimeEngine.PushValue( (long)n0 );
                    }
                    break;
                case char c:
                    var n1 = subject.IndexOf( c );
                    if (n1 < 0) {
                        throw new NutmegException( "Cannot find index" ).Culprit( "Subject", subject ).Culprit( "Argument", $"{c}" );
                    } else {
                        runtimeEngine.PushValue( (long)n1 );
                    }
                    break;
                default:
                    throw new NutmegException( "Invalid argument for indexOf" )
                        .Culprit( "Argument", $"{target}")
                        .Hint( "Argument must be a char or a string" );
            }

            return this.Next;
        }

    }

    public class StringAppend : FixedAritySystemFunction {

        public StringAppend( Runlet next ) : base( next ) {
        }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            string obj = (string)runtimeEngine.PopValue();
            string subject = (string)runtimeEngine.PopValue();
            runtimeEngine.PushValue( subject + obj );
            return this.Next;
        }

    }

    public class StringNewString : VariadicSystemFunction {

        public StringNewString( Runlet next ) : base( next ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int n = runtimeEngine.ValueStackLength();
            var b = new StringBuilder();
            for ( int i = 0; i < n; i++ ) {
                var item = runtimeEngine.GetItem( i );
                switch ( item ) {
                    case char ch:
                        b.Append( ch );
                        break;
                    case string s:
                        b.Append( s );
                        break;
                    default:
                        throw new NutmegException( "Invalid argument for newString" ).Culprit( "Argument", $"{item}" );
                }
            }
            runtimeEngine.ClearValueStack();
            runtimeEngine.PushValue( b.ToString() );
            return this.Next;
        }
    }

    public class StringSplit : FixedAritySystemFunction {

        public StringSplit( Runlet next ) : base( next ) {
        }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            string s = (string)runtimeEngine.PopValue();
            foreach ( var i in s.Split() ) {
                runtimeEngine.PushValue( i );
            }
            return this.Next;
        }

    }

    public class StringJoin : VariadicSystemFunction {

        public StringJoin( Runlet next ) : base( next ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            StringBuilder b = new StringBuilder();
            int n = runtimeEngine.ValueStackLength();
            string sep = (string)runtimeEngine.GetItem( 0 );
            for ( int i = 1; i < n; i++ ) {
                if ( i > 1 ) {
                    b.Append( sep );
                }
                string t = (string)runtimeEngine.GetItem( i );
                b.Append( t );
            }
            runtimeEngine.ClearValueStack();
            runtimeEngine.PushValue( b.ToString() );
            return this.Next;
        }
    }

    public class StringSubstring : VariadicSystemFunction {

        public StringSubstring( Runlet next ) : base( next ) {
        }

        private static void GeneralAppend( StringBuilder b, string s, object x ) {
            switch (x) {
                case HalfOpenRangeList r:
                    b.Append( s, (int)r.Low, (int)(r.High - r.Low) );
                    break;
                default:
                    var stream = StreamSystemFunction.ToStream( x );
                    while (stream.MoveNext()) {
                        var index = (int)(long)stream.Current;
                        b.Append( s[index] );
                    }
                    break;
            }
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            int N = runtimeEngine.ValueStackLength();
            switch (N) {
                case 0:
                    throw new NutmegException( "No arguments supplied to substring" ).Hint( "At least one is required" );
                case 1:
                    runtimeEngine.ClearValueStack();
                    runtimeEngine.PushValue( "" );
                    break;
                case 2:
                    //  This will be the common case and it has an important efficiency advantage, so has to be broken out.
                    {
                        object x = runtimeEngine.PopValue();
                        string s = (string)runtimeEngine.PopValue();
                        switch (x) {
                            case HalfOpenRangeList r:
                                var t = s.Substring( (int)r.Low, (int)(r.High - r.Low) );
                                runtimeEngine.PushValue( t );
                                break;
                            default:
                                var b = new StringBuilder();
                                GeneralAppend( b, s, x );
                                runtimeEngine.PushValue( b.ToString() );
                                break;
                        }
                    }
                    break;
                default:
                    {
                        var b = new StringBuilder();
                        var s = (string)runtimeEngine.GetItem( 0 );
                        for (int i = 1; i < N; i++) {
                            var x = runtimeEngine.GetItem( i );
                            GeneralAppend( b, s, x );
                        }
                        runtimeEngine.ClearValueStack();
                        runtimeEngine.PushValue( b.ToString() );
                    }
                    break;
            }
            return this.Next;
        }

    }


    public class StringsModule : SystemFunctionsModule {

        public override void AddAll() {
            Add( "newString", r => new StringNewString( r ) );
            Add( "length", r => new StringLength( r ) );
            Add( "get", r => new StringGet( r ) );
            Add( "startsWith", r => new StringStartsWith( r ) );
            Add( "endsWith", r => new StringEndsWith( r ) );
            Add( "contains", r => new StringContains( r ) );
            Add( "trim", r => new StringTrim( r ) );
            Add( "++", r => new StringAppend( r ) );
            Add( "split", r => new StringSplit( r ) );
            Add( "join", r => new StringJoin( r ) );
            Add( "substring", r => new StringSubstring( r ) );
        }

    }
}

using System.Text;

namespace NutmegRunner.Modules.Strings {

    public class StringLength : FixedAritySystemFunction {

        public StringLength( Runlet next ) : base( next ) {
        }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            string x = (string)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x.Length );
            return this.Next;
        }

    }

    public class StringGet : FixedAritySystemFunction {

        public StringGet( Runlet next ) : base( next ) {
        }

        public override int Nargs => 2;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            long n = (long)runtimeEngine.PopValue();
            string x = (string)runtimeEngine.PopValue();
            runtimeEngine.PushValue( x[(int)n] );
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

    public class StringIsSubstring : FixedAritySystemFunction {

        public StringIsSubstring( Runlet next ) : base( next ) {
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

    public class StringsModule : SystemFunctionsModule {

        public override void AddAll() {
            Add( "newString", r => new StringNewString( r ) );
            Add( "length", r => new StringLength( r ) );
            Add( "get", r => new StringGet( r ) );
            Add( "startsWith", r => new StringStartsWith( r ) );
            Add( "endsWith", r => new StringEndsWith( r ) );
            Add( "contains", r => new StringIsSubstring( r ) );
            Add( "trim", r => new StringTrim( r ) );
            Add( "++", r => new StringAppend( r ) );
        }

    }
}

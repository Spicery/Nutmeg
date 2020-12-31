using System;
using System.Linq;

namespace NutmegRunner.Modules.Characters {

    public class IsLowercase : FixedAritySystemFunction {

        public IsLowercase( Runlet next ) : base( next ) {
        }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var x = runtimeEngine.PopValue();
            switch (x) {
                case char c:
                    runtimeEngine.PushValue( Char.IsLower( c ) );
                    break;
                case string s:
                    runtimeEngine.PushValue( s.All<char>( ch => !Char.IsUpper( ch ) ) );
                    break;
                default:
                    throw new NutmegException( "Invalid argument for isLowercase" ).Hint( "string or character needed" ).Culprit( "Argument", $"{x}" );
            }
            return this.Next;
        }

    }

    public class IsUppercase : FixedAritySystemFunction {

        public IsUppercase( Runlet next ) : base( next ) {
        }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var x = runtimeEngine.PopValue();
            switch (x) {
                case char c:
                    runtimeEngine.PushValue( Char.IsUpper( c ) );
                    break;
                case string s:
                    runtimeEngine.PushValue( s.All<char>( ch => !Char.IsLower( ch ) ) );
                    break;
                default:
                    throw new NutmegException( "Invalid argument for isUppercase" ).Hint( "string or character needed" ).Culprit( "Argument", $"{x}" );
            }
            return this.Next;
        }

    }

    public class Lowercase : FixedAritySystemFunction {

        public Lowercase( Runlet next ) : base( next ) {
        }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var x = runtimeEngine.PopValue();
            switch (x) {
                case char c:
                    runtimeEngine.PushValue( Char.ToLower( c, System.Globalization.CultureInfo.InvariantCulture ) );
                    break;
                case string s:
                    runtimeEngine.PushValue( s.ToLower() );
                    break;
                default:
                    throw new NutmegException( "Invalid argument for lowercase" ).Hint( "string or character needed" ).Culprit( "Argument", $"{x}" );
            }
            return this.Next;
        }

    }

    public class Uppercase : FixedAritySystemFunction {

        public Uppercase( Runlet next ) : base( next ) {
        }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var x = runtimeEngine.PopValue();
            switch (x) {
                case char c:
                    runtimeEngine.PushValue( Char.ToUpper( c, System.Globalization.CultureInfo.InvariantCulture ) );
                    break;
                case string s:
                    runtimeEngine.PushValue( s.ToUpper() );
                    break;
                default:
                    throw new NutmegException( "Invalid argument for uppercase" ).Hint( "string or character needed" ).Culprit( "Argument", $"{x}" );
            }
            return this.Next;
        }

    }

    public class CharactersModule : SystemFunctionsModule {
        public override void AddAll() {
            Add( "lowercase", r => new Lowercase( r ) );
            Add( "uppercase", r => new Uppercase( r ) );
        }
    }

}

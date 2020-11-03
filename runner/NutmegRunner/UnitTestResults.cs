using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace NutmegRunner {

    public class UnitTestResults {

        private const string red = "RED";
        private const string amber = "AMBER";
        private const string green = "GREEN";

        private SQLiteConnection _connection;
        private List<string> _passes = new List<string>();
        private List<Tuple<string, Exception>> _failures = new List<Tuple<string, Exception>>();

        public UnitTestResults( SQLiteConnection connection ) {
            this._connection = connection;
        }

        public void AddPass( string idname ) {
            _passes.Add( idname );
        }

        public void AddFailure( string idname, Exception exn ) {
            _failures.Add( new Tuple<string, Exception>( idname, exn ) );
        }

        public string Status() {
            return (
                _failures.Count > 0 ? red :
                _passes.Count == 0 ? amber :
                green
            );
        }

        public void ShowResults() {
            Console.WriteLine( $"{Status()}: {_passes.Count} passed, {_failures.Count} failed" );
            if (_failures.Count > 0) {
                int n = 0;
                foreach (var f in _failures) {
                    n += 1;
                    Exception e = f.Item2;
                    string msg = GetAssertFailureMessage( _connection, e );
                    Console.WriteLine( $"[{n}] {f.Item1}, {msg}" );
                    if (e is AssertionFailureException exn) {
                        foreach (var culprit in exn.Culprits) {
                            Console.WriteLine( $"  - {culprit.Key}: {culprit.Value}" );
                        }
                    }
                }
            }
        }

        private string GetAssertFailureMessage( SQLiteConnection connection, Exception ex ) {
            var msg = ex.Message;

            if (ex is AssertionFailureException exn) {
                int pos = (int)exn.Position;
                if (TryGetLine( connection, exn.Unit, pos, out var line )) {
                    int n = line.IndexOf( '\n' );
                    if (n < 0) {
                        n = line.Length;
                    }
                    msg = line.Substring( 0, n );
                }
            }

            return msg;
        }

        private bool TryGetLine( SQLiteConnection connection, string unit, int posn, out string line ) {
            line = null;
            if (unit == null) return false;
            using (SQLiteCommand cmd = new SQLiteCommand( "SELECT 1 + length(substr(Contents, 0, @Posn)) - length(replace(substr(Contents, 0, @Posn), CHAR(10), '')), substr( Contents, @Posn, 80 ) FROM SourceFiles WHERE FileName = @Unit", connection )) {
                cmd.Parameters.AddWithValue( "@Posn", posn + 1 );   //  Add 1 to compensate for the 1-indexing of substr.
                cmd.Parameters.AddWithValue( "@Unit", unit );
                cmd.Prepare();
                var reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    var lineno = reader.GetInt32( 0 );
                    var text = reader.GetString( 1 );
                    var fname = unit.Substring( unit.LastIndexOf( '/' ) + 1 );
                    line = $"line {lineno} of {fname}: {text}";
                }
            }
            return line != null;
        }

    }

}

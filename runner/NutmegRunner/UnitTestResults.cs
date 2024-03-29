﻿using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace NutmegRunner {

    public class UnitTestResults {

        public const string red = "RED";
        public const string amber = "AMBER";
        public const string green = "GREEN";

        private SqliteConnection _connection;
        private List<string> _passes = new List<string>();
        private List<Tuple<string, Exception>> _failures = new List<Tuple<string, Exception>>();
        private string _title;

        public UnitTestResults( string title, SqliteConnection connection ) {
            this._title = title;
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

        public bool NoFailures() {
            return _failures.Count == 0;
        }

        public void ShowResults() {
            Console.Write( $"{Status()}: {_passes.Count} passed, {_failures.Count} failed" );
            if ( ! string.IsNullOrWhiteSpace( this._title ) ) {
                Console.Write( $" [{this._title}]" );
            }
            Console.WriteLine();
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

        private string GetAssertFailureMessage( SqliteConnection connection, Exception ex ) {
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

        private bool TryGetLine( SqliteConnection connection, string unit, int posn, out string line ) {
            line = null;
            if (unit == null) return false;
            using (SqliteCommand cmd = new SqliteCommand( "SELECT 1 + length(substr(Contents, 0, @Posn)) - length(replace(substr(Contents, 0, @Posn), CHAR(10), '')), substr( Contents, @Posn, 80 ) FROM SourceFiles WHERE FileName = @Unit", connection )) {
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

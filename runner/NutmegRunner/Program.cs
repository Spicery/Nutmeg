﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace NutmegRunner {

    class Program {

        string _bundleFile;
        string _entryPoint = null;
        bool _debug = false;
        string _graphviz = null;
        bool _print = false;
        bool _test = false;

        /// <summary>
        /// This constructor is responsible for parsing the arguments into instance variables.
        /// To start with on form is:
        ///     nutmeg --entry-point=NAME BUNDLE_FILE PROGRAM_ARGS...
        /// We only have 1 interpreter option defined so far, which is --entry-point, which
        /// defaults to the identifier "program".
        /// </summary>
        /// <param name="args"></param>
        private Program(LinkedList<string> args) {
            ProcessRunnerOptions( args );
            ProcessBundleFile( args );
        }

        private void ProcessBundleFile( LinkedList<string> args ) {
            if (args.Count < 1) throw new NutmegException( "No bundle-file specified" );
            this._bundleFile = args.First.Value;
            args.RemoveFirst();
        }

        private void ProcessRunnerOptions( LinkedList<string> args ) {
            //Console.WriteLine( $"Show Options" );
            //foreach (var arg in args) {
            //    Console.WriteLine( $"arg = {arg}" );
            //}
            while (args.Count > 0 && args.First.Value.Length >= 2 && args.First.Value.StartsWith( "-" )) {
                var option = args.First.Value;
                //Console.WriteLine( $"option = {option}" );
                args.RemoveFirst();
                var n = option.IndexOf( '=' );
                if (option.StartsWith( "--" )) {
                    //  Long option processing.
                    string parameter = null;
                    if (n != -1) {
                        parameter = option.Substring( n + 1 );
                        option = option.Substring( 0, n );
                    }
                    switch (option) {
                        case "--entry-point":
                            if (parameter != null) {
                                this._entryPoint = parameter;
                            } else {
                                throw new UsageNutmegException();
                            }
                            break;
                        case "--print":
                            this._print = true;
                            break;
                        case "--debug":
                            this._debug = true;
                            break;
                        case "--graphviz":
                            this._graphviz = parameter;
                            break;
                        case "--test":
                            this._test = true;
                            break;
                        default:
                            throw new NutmegException( $"Unrecognised option: {option}" ).Culprit( "Option", option );
                    }
                } else {
                    //  Short option processing: works by expanding into long options.
                    var compactOption = option.Substring( 1 );
                    while (compactOption.Length > 0) {
                        if (compactOption.StartsWith( "p" )) {
                            args.AddBefore( args.First, "--print" );
                            compactOption = compactOption.Substring( 1 );
                        } else if (compactOption.StartsWith( "d" )) {
                            args.AddBefore( args.First, "--debug" );
                            compactOption = compactOption.Substring( 1 );
                        } else {
                            throw new NutmegException( $"Cannot parse compact command-line option: {compactOption}" ).Culprit( "Option", option );
                        }
                    }
                }
            }
        }

        private IEnumerable<string> GetEntryPoints( SQLiteConnection connection ) {
            var cmd = new SQLiteCommand( "SELECT [IdName] FROM [EntryPoints]", connection );
            var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                yield return reader.GetString( 0 );
            }
        }

        private void Run()
        {
            TextWriter stdErr = Console.Error;
            if (this._debug) {
                stdErr.WriteLine( "Nutmeg kicks the ball ..." );
                stdErr.WriteLine( $"Bundle file: {this._bundleFile}" );
                stdErr.WriteLine( $"Entry point: {this._entryPoint}" );
                stdErr.WriteLine( $"Unittests: {this._test}" );
            }
            try {
                RuntimeEngine runtimeEngine = new RuntimeEngine( this._debug );
                using (SQLiteConnection connection = GetConnection()) {
                    connection.Open();
                    if (this._entryPoint == null && !this._test) {
                        //  If the entry-point is not specified, and we need an entry-point (i.e. not a test run), check if
                        //  there a unique entry-point in the bundle.
                        var entrypoints = this.GetEntryPoints( connection ).ToList();
                        var n = entrypoints.Count();
                        if (n == 1) {
                            this._entryPoint = entrypoints.First();
                            if (this._debug) stdErr.WriteLine( $"Inferred entry point: {this._entryPoint}" );
                        } else {
                            throw new NutmegException( "Cannot determine the entry-point" ).Hint( n == 0 ? "No default entry-point" : "More than one entry-point" );
                        }
                    }
                    SQLiteCommand cmd = GetCommandForBindingsToLoad( connection );
                    var reader = cmd.ExecuteReader();
                    var bindings = new Dictionary<string, Codelet>();
                    var initialisations = new List<KeyValuePair<string, Codelet>>();
                    while (reader.Read()) {
                        string idName = reader.GetString( 0 );
                        if (this._debug) Console.WriteLine( $"Loading {idName}" );
                        string jsonValue = reader.GetString( 1 );
                        if (this._debug) stdErr.WriteLine( $"Loading definition: {idName}" );
                        try {
                            Codelet codelet = Codelet.DeserialiseCodelet( jsonValue );
                            runtimeEngine.PreBind( idName );
                            if (codelet is LambdaCodelet fc) {
                                bindings.Add( idName, codelet );
                            } else if (codelet is SysfnCodelet sfc) {
                                bindings.Add( idName, codelet );
                            } else {
                                initialisations.Add( new KeyValuePair<string, Codelet>( idName, codelet ) );
                            }
                        } catch (Newtonsoft.Json.JsonSerializationException e) {
                            Exception inner = e.InnerException;
                            throw (inner is NutmegException nme) ? (Exception)nme : (Exception)e;
                        }
                    }
                    foreach (var k in bindings) {
                        runtimeEngine.Bind( k.Key, k.Value );
                    }
                    foreach (var kvp in initialisations) {
                        if (this._debug) Console.WriteLine( $"Binding {kvp.Key}" );
                        try {
                            runtimeEngine.Initialise( kvp.Key, kvp.Value );
                        } catch (NormalExitNutmegException) {
                            //  This is how initialisation is halted.
                        }
                    }
                }
                if (this._graphviz != null) {
                    runtimeEngine.GraphViz( this._graphviz );
                } else if ( this._test ) {
                    using (SQLiteConnection connection = GetConnection()) {
                        connection.Open();
                        var tests_to_run = GetTestsToRun( connection );
                        var passes = new List<string>();
                        var failures = new List<Tuple<string, Exception>>();
                        foreach (var idName in tests_to_run) {
                            if (this._debug) Console.WriteLine( $"Running unit test for {idName}" );
                            try {
                                runtimeEngine.Start( idName, useEvaluate: false, usePrint: this._print );
                                passes.Add( idName );
                            } catch (Exception ex) {
                                failures.Add( new Tuple<string, Exception>( idName, ex ) );
                            } finally {
                                runtimeEngine.Reset();
                            }
                        }
                        var r_a_g = failures.Count == 0 ? "GREEN" : "RED";
                        Console.WriteLine( $"{r_a_g}: {passes.Count} passed, {failures.Count} failed" );
                        if (failures.Count > 0) {
                            foreach (var f in failures) {
                                string msg = GetAssertFailureMessage( connection, f.Item2 );
                                Console.WriteLine( $" * {f.Item1}: {msg}" );
                            }
                        }
                    }
                } else {
                    runtimeEngine.Start( this._entryPoint, useEvaluate: false, usePrint: this._print );
                }
            } catch (NutmegException nme) {
                if ( this._debug ) Console.Error.WriteLine( $"MISHAP: {nme.Message}" );
                foreach (var culprit in nme.Culprits) {
                    Console.Error.WriteLine( $" {culprit.Key}: {culprit.Value}" );
                }
                throw nme;  // rethrow
            }
        }

        private string GetAssertFailureMessage( SQLiteConnection connection, Exception ex ) {
            var msg = ex.Message;
            if (ex is NutmegTestFailException exn ) {
                int pos = (int)exn.Position;
                if (TryGetLine( connection, exn.Unit, pos, out var line )) {
                    int n = line.IndexOf( '\n' );
                    if ( n < 0 ) {
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
            using (SQLiteCommand cmd = new SQLiteCommand( "SELECT substr( Contents, @Posn, 80 ) FROM SourceFiles WHERE FileName = @Unit", connection )) {
                cmd.Parameters.AddWithValue( "@Posn", posn + 1 );   //  Add 1 to compensate for the 1-indexing of substr.
                cmd.Parameters.AddWithValue( "@Unit", unit );
                cmd.Prepare();
                var reader = cmd.ExecuteReader();
                if ( reader.Read() ) {
                    line = reader.GetString( 0 );
                }
            }
            return line != null;
        }

        private List<string> GetTestsToRun( SQLiteConnection connection ) {
            var sofar = new List<string>();
            using (var cmd = new SQLiteCommand( "SELECT B.[IdName] FROM [Bindings] B JOIN [Annotations] A ON A.IdName = B.IdName WHERE A.AnnotationKey='unittest'", connection )) {
                var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    string idName = reader.GetString( 0 );
                    sofar.Add( idName );
                }
                return sofar;
            }
        }

        private SQLiteCommand GetCommandForBindingsToLoad( SQLiteConnection connection ) {
            if ( this._test ) {
                var cmd = new SQLiteCommand( "SELECT B.[IdName], B.[Value] FROM [Bindings] B JOIN [Annotations] A ON A.IdName = B.IdName JOIN [DependsOn] E ON E.[Needs] = B.[IdName] WHERE A.AnnotationKey='unittest'", connection );
                cmd.Prepare();
                return cmd;
            } else {
                var cmd = new SQLiteCommand( "SELECT B.[IdName], B.[Value] FROM [Bindings] B JOIN [DependsOn] E ON E.[Needs] = B.[IdName] WHERE E.[IdName]=@EntryPoint", connection );
                cmd.Parameters.AddWithValue( "@EntryPoint", this._entryPoint );
                cmd.Prepare();
                return cmd;
            }
        }

        private SQLiteConnection GetConnection() {
            try {
                return new SQLiteConnection( $"Data Source={this._bundleFile}; Read Only=True;" );
            } catch (System.Data.SQLite.SQLiteException) {
                throw new NutmegException( "Cannot find/open bundle" ).Culprit( "Filename", this._bundleFile );
            }
        }

        static void Main(string[] args) {
            new Program( new LinkedList<string>( args ) ).Run();
        }
    }
}

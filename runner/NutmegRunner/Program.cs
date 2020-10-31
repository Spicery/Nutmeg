using System;
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
            while (args.Count > 0 && args.First.Value.Length >= 2 && args.First.Value.StartsWith( "-" )) {
                var option = args.First.Value;
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
            var cmd = new SQLiteCommand( "SELECT [IdName] FROM [Annotations] WHERE AnnotationKey='command'", connection );
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
                        UnitTestResults utresults = new UnitTestResults( connection );
                        foreach (var idName in tests_to_run) {
                            if (this._debug) Console.WriteLine( $"Running unit test for {idName}" );
                            try {
                                runtimeEngine.Start( idName, useEvaluate: false, usePrint: this._print );
                                utresults.AddPass( idName );
                            } catch (Exception ex) {
                                utresults.AddFailure( idName, ex );
                            } finally {
                                runtimeEngine.Reset();
                            }
                        }
                        utresults.ShowResults();
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
                var cmd = new SQLiteCommand( "SELECT B.[IdName], B.[Value] FROM [Bindings] B JOIN [DependsOn] E ON E.[Needs] = B.[IdName] JOIN [Annotations] A ON A.[IdName] = E.[IdName] WHERE A.AnnotationKey='unittest'", connection );
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

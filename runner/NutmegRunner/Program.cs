using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace NutmegRunner {

    delegate SqliteConnection GetSqliteConnection();

    class UnitTestRunner {

        RuntimeEngine _runtimeEngine;
        GetSqliteConnection _getConnection;
        string _testTitle;

        public UnitTestRunner( string testTitle, RuntimeEngine runtimeEngine, GetSqliteConnection getConnection  ) {
            this._testTitle = testTitle;
            this._runtimeEngine = runtimeEngine;
            this._getConnection = getConnection;
        }

        public bool Debug => this._runtimeEngine.Debug;

        public void RunTests() { 
            using (SqliteConnection connection = this._getConnection()) {
                connection.Open();
                var tests_to_run = this.GetTestsToRun( connection );
                UnitTestResults utresults = new UnitTestResults( this._testTitle, connection );
                foreach (var idName in tests_to_run) {
                    if (this.Debug ) Console.WriteLine( $"Running unit test for {idName}" );
                    try {
                        this._runtimeEngine.Start( idName, new List<string>(), useEvaluate: false, usePrint: false );
                        utresults.AddPass( idName );
                    } catch (Exception ex) {
                        utresults.AddFailure( idName, ex );
                    } finally {
                        this._runtimeEngine.Reset();
                    }
                }
                utresults.ShowResults();
            }
        }

        private List<string> GetTestsToRun( SqliteConnection connection ) {
            var sofar = new List<string>();
            using (var cmd = new SqliteCommand( "SELECT B.[IdName] FROM [Bindings] B JOIN [Annotations] A ON A.IdName = B.IdName WHERE A.AnnotationKey='unittest'", connection )) {
                var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    string idName = reader.GetString( 0 );
                    sofar.Add( idName );
                }
                return sofar;
            }
        }

    }

    class Program {

        string _bundleFile;
        string _entryPoint = null;
        bool _debug = false;
        bool _trace = false;
        string _graphviz = null;
        bool _print = false;
        bool _test = false;
        string _title = null;
        bool _info = false;

        LinkedList<string> _args;

        public bool InTestMode => this._test;
        public string TestTitle => this._title;

        public bool Trace => this._trace;

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
            this._args = args;
        }

        private void ProcessBundleFile( LinkedList<string> args ) {
            if (args.Count >= 1) {
                this._bundleFile = args.First.Value;
                args.RemoveFirst();
            } else if (! this._info) {
                throw new NutmegException( "No bundle-file specified" );
            }
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
                        case "--debug":
                            this._debug = true;
                            this._trace = true;
                            break;
                        case "--info":
                            this._info = true;
                            break;
                        case "--graphviz":
                            this._graphviz = parameter;
                            break;
                        case "--print":
                            this._print = true;
                            break;
                        case "--title":
                            this._title = parameter;
                            break;
                        case "--trace":
                            this._trace = true;
                            break;
                        case "--unittest":
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
                        } else if (compactOption.StartsWith( "t" )) {
                            args.AddBefore( args.First, "--trace" );
                            compactOption = compactOption.Substring( 1 );
                        } else {
                            throw new NutmegException( $"Cannot parse compact command-line option: {compactOption}" ).Culprit( "Option", option );
                        }
                    }
                }
            }
        }

        private IEnumerable<string> GetEntryPoints( SqliteConnection connection ) {
            var cmd = new SqliteCommand( "SELECT [IdName] FROM [Annotations] WHERE AnnotationKey='command'", connection );
            var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                yield return reader.GetString( 0 );
            }
        }

        private void Info() {
            Console.WriteLine(
                JsonConvert.SerializeObject(
                    new {
                        SystemFunctions = NutmegSystem.SystemInfo().ToDictionary( x => x.Name )
                    },
                    Formatting.Indented
                )
            );
        }

        private void Run() {
            TextWriter stdErr = Console.Error;
            if (this._debug) {
                stdErr.WriteLine( "Nutmeg kicks the ball ..." );
                stdErr.WriteLine( $"Bundle file: {this._bundleFile}" );
                stdErr.WriteLine( $"Entry point: {this._entryPoint}" );
                stdErr.WriteLine( $"Unittests: {this.InTestMode}" );
                stdErr.WriteLine( $"GraphViz: {this._graphviz}" );
                stdErr.WriteLine( $"Info: {this._info}" );
            }
            if (this._info) {
                this.Info();
            } else {
                this.RunProgram();
            }
        }

        private void RunProgram() {
            TextWriter stdErr = Console.Error;
            try {
                RuntimeEngine runtimeEngine = new RuntimeEngine( this._debug );
                using (SqliteConnection connection = GetConnection()) {
                    connection.Open();
                    if (this._entryPoint == null && !this.InTestMode) {
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
                    SqliteCommand cmd = GetCommandForBindingsToLoad( connection );
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
                } else if (this.InTestMode) {
                    UnitTestRunner utrunner = new( this.TestTitle, runtimeEngine, this.GetConnection );
                    utrunner.RunTests();
                } else {
                    runtimeEngine.Start( this._entryPoint, this._args, useEvaluate: false, usePrint: this._print );
                }
            } catch (SqliteException sqlexn) {
                if (sqlexn.SqliteErrorCode == 26) {
                    throw new NutmegException( "Supplied bundle-file has wrong format" )
                        .Culprit( "File", this._bundleFile )
                        .Hint( "Bundle files are SQLITE databases" );
                } else {
                    throw new NutmegException( "Problem with the bundle file", sqlexn );
                }
            }
        }



        private SqliteCommand GetCommandForBindingsToLoad( SqliteConnection connection ) {
            if ( this.InTestMode ) {
                var cmd = new SqliteCommand( "SELECT DISTINCT B.[IdName], B.[Value] FROM [Bindings] B JOIN [DependsOn] E ON E.[Needs] = B.[IdName] JOIN [Annotations] A ON A.[IdName] = E.[IdName] WHERE A.AnnotationKey='unittest'", connection );
                cmd.Prepare();
                return cmd;
            } else {
                var cmd = new SqliteCommand( "SELECT B.[IdName], B.[Value] FROM [Bindings] B JOIN [DependsOn] E ON E.[Needs] = B.[IdName] WHERE E.[IdName]=@EntryPoint", connection );
                cmd.Parameters.AddWithValue( "@EntryPoint", this._entryPoint );
                cmd.Prepare();
                return cmd;
            }
        }

        private SqliteConnection GetConnection() {
            try {
                return new SqliteConnection( new SqliteConnectionStringBuilder() { DataSource = this._bundleFile, Mode = SqliteOpenMode.ReadOnly }.ToString() );
            } catch (Microsoft.Data.Sqlite.SqliteException) {
                throw new NutmegException( "Cannot find/open bundle" ).Culprit( "Filename", this._bundleFile );
            }
        }

        static void Main( string[] args ) {
            var program = new Program( new LinkedList<string>( args ) );
            try {
                program.Run();
            } catch (NutmegException nme) {
                nme.WriteMessage();
                if (program.Trace) throw;
                Environment.Exit( -1 );
            }
        }
    }
}

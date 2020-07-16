using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace NutmegRunner {

    class Program {

        string _bundleFile;
        string _entryPoint = "program";
        bool _debug = false;

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
            while (args.Count > 0 && args.First.Value.StartsWith( "--" )) {
                var option = args.First.Value;
                args.RemoveFirst();
                var n = option.IndexOf( '=' );
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
                        break;
                    default:
                        throw new NutmegException( "Unrecognised option" ).Culprit( "Option", option );
                }
            }
        }

        private void Run()
        {
            TextWriter stdErr = Console.Error;
            if (this._debug) stdErr.WriteLine("Nutmeg kicks the ball ...");
            if (this._debug) stdErr.WriteLine( $"Bundle file: {this._bundleFile}" );
            if (this._debug) stdErr.WriteLine( $"Entry point: {this._entryPoint}" );
            RuntimeEngine runtimeEngine = new RuntimeEngine( this._debug );
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={this._bundleFile}"))
            {
                connection.Open();
                var cmd = new SQLiteCommand( "SELECT B.[IdName], B.[Value] FROM [Bindings] B JOIN [EntryPoints] E ON E.[Needs] = B.[IdName] WHERE E.[EntryPoint]=@EntryPoint", connection);
                cmd.Parameters.AddWithValue( "@EntryPoint", this._entryPoint );
                cmd.Prepare();
                var reader = cmd.ExecuteReader();
                var bindings = new Dictionary<string, Codelet>();
                while (reader.Read()) {
                    string idName = reader.GetString( 0 );
                    string jsonValue = reader.GetString( 1 );
                    if (this._debug) stdErr.WriteLine( $"Loading definition: {idName}" );
                    Codelet codelet = Codelet.DeserialiseCodelet( jsonValue );
                    bindings.Add( idName, codelet );
                    runtimeEngine.PreBind( idName );
                }
                foreach (var k in bindings) {
                    runtimeEngine.Bind( k.Key, k.Value );
                }
            }
            runtimeEngine.Start( this._entryPoint, useEvaluate: false );

            //var jobj = JToken.ReadFrom(new JsonTextReader(new StreamReader(Console.OpenStandardInput())));
            //Console.WriteLine($"Output = {jobj.ToString()}");
        }

        static void Main(string[] args) {
            new Program( new LinkedList<string>( args ) ).Run();
        }
    }
}

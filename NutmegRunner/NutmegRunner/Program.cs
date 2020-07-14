using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace NutmegRunner {

    class Program {

        string bundleFile;
        string entryPoint = "program";

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
            this.bundleFile = args.First.Value;
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
                            this.entryPoint = parameter;
                        } else {
                            throw new UsageNutmegException();
                        }
                        break;
                    default:
                        throw new NutmegException( "Unrecognised option" ).Culprit( "Option", option );
                }
            }
        }

        private void Run()
        {
            var debug = false;
            TextWriter stdErr = Console.Error;
            if ( debug ) stdErr.WriteLine("Nutmeg kicks the ball ...");
            if (debug) stdErr.WriteLine( $"bundle file: {this.bundleFile}" );
            if (debug) stdErr.WriteLine( $"entry point: {this.entryPoint}" );
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source={this.bundleFile}"))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT B.[IdName], B.[Value] FROM [Bindings] B JOIN [EntryPoints] E ON E.[IdName] = B.[IdName] WHERE E.[IdName]=@EntryPoint", connection);
                cmd.Parameters.AddWithValue( "@EntryPoint", this.entryPoint );
                cmd.Prepare();
                var reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    string idName = reader.GetString( 0 );
                    string jsonValue = reader.GetString( 1 );
                    Codelet codelet = Codelet.DeserialiseCodelet( jsonValue );
                    RuntimeEngine runtimeEngine = new RuntimeEngine();
                    runtimeEngine.Bind( idName, codelet );
                    runtimeEngine.Start( idName, useEvaluate: false, debug: true );
                } else {
                    stdErr.WriteLine( "No such entry point, so sorry" );
                }
            }

            //var jobj = JToken.ReadFrom(new JsonTextReader(new StreamReader(Console.OpenStandardInput())));
            //Console.WriteLine($"Output = {jobj.ToString()}");
        }

        static void Main(string[] args) {
            new Program( new LinkedList<string>( args ) ).Run();
        }
    }
}

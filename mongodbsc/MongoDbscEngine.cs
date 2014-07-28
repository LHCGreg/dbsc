using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using MongoDB.Driver;
using System.Globalization;
using System.Diagnostics;
using System.Linq.Expressions;
using System.IO;

namespace dbsc.Mongo
{
    class MongoDbscEngine
        : DbscEngine<DbConnectionInfo, MongoCheckoutOptions, ImportSettingsWithTableList<DbConnectionInfo>, MongoUpdateOptions>
        , IDbscEngineWithTableImport<DbConnectionInfo, ImportSettingsWithTableList<DbConnectionInfo>, MongoUpdateOptions>
    {
        protected override string ScriptExtensionWithoutDot { get { return "js"; } }

        protected override bool CheckoutAndUpdateIsSupported(out string whyNot)
        {
            if (!Utils.ExecutableIsOnPath("mongo", "--version"))
            {
                whyNot = "You must have the mongo command line program installed and on your PATH to check out and update databases.";
                return false;
            }
            else
            {
                whyNot = null;
                return true;
            }
        }

        protected override bool ImportIsSupported(out string whyNot)
        {
            if (!Utils.ExecutableIsOnPath("mongodump", "--version"))
            {
                whyNot = "You must have mongodump installed and on your PATH to import data from another database.";
                return false;
            }
            else
            {
                whyNot = null;
                return true;
            }
        }

        private const string MetadataCollectionName = "dbsc_metadata";

        protected override bool DatabaseHasMetadataTable(DbConnectionInfo connectionInfo)
        {
            using (MongoDbscConnection conn = new MongoDbscConnection(connectionInfo))
            {
                return conn.ContainsCollection(MetadataCollectionName);
            }
        }

        protected override void CreateDatabase(MongoCheckoutOptions options)
        {
            DbConnectionInfo adminDb = options.TargetDatabase.Clone();
            adminDb.Database = "admin";

            Console.WriteLine("Creating database {0} on {1}.", options.TargetDatabase.Database, options.TargetDatabase.Server);
            using (MongoDbscConnection conn = new MongoDbscConnection(adminDb))
            {
                conn.CreateDatabase(options.TargetDatabase.Database);
            }
            Console.WriteLine("Created database {0} on {1}.", options.TargetDatabase.Database, options.TargetDatabase.Server);

            if (options.CreationTemplate != null)
            {
                string tempFilePath = Path.GetTempFileName();
                try
                {
                    string creationScript = options.CreationTemplate.Replace("$DatabaseName$", options.TargetDatabase.Database);
                    using (StreamWriter tempFileWriter = new StreamWriter(tempFilePath))
                    {
                        tempFileWriter.Write(creationScript);
                    }
                    // Must close the file before mongo can read it because mongo opens it
                    // without the equivalent of FileShare.Write.
                    RunScript(options.UpdateOptions, tempFilePath);
                }
                finally
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch
                    {
                        ;
                    }
                }
            }
        }

        protected override void InitializeDatabase(MongoCheckoutOptions options, string masterDatabaseName)
        {
            using (MongoDbscConnection conn = new MongoDbscConnection(options.TargetDatabase))
            {
                conn.CreateCollection(MetadataCollectionName);
                dbsc_metadata metadata = new dbsc_metadata(-1, masterDatabaseName, DateTime.UtcNow);
                conn.Upsert(metadata, MetadataCollectionName);
            }
        }

        protected override int GetRevision(DbConnectionInfo connectionInfo)
        {
            using (MongoDbscConnection conn = new MongoDbscConnection(connectionInfo))
            {
                dbsc_metadata metadata = conn.GetSingleDocument<dbsc_metadata>(MetadataCollectionName);
                return metadata.Version;
            }
        }

        protected override void RunScriptAndUpdateMetadata(MongoUpdateOptions options, string scriptPath, int newRevision)
        {
            RunScript(options, scriptPath);
            UpdateMetadata(options, newRevision, DateTime.UtcNow);
        }

        private void RunScript(MongoUpdateOptions options, string scriptPath)
        {
            string mongoArgs = GetMongoArgs(options, scriptPath: scriptPath);
            Process mongo = new Process()
            {
                StartInfo = new ProcessStartInfo("mongo", mongoArgs)
                {
                    ErrorDialog = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            object consoleLock = new object();
            mongo.OutputDataReceived += (sender, e) => { lock (consoleLock) { Console.WriteLine(e.Data); } };
            mongo.ErrorDataReceived += (sender, e) => { lock (consoleLock) { Console.WriteLine(e.Data); } };
            using (mongo)
            {
                mongo.Start();
                mongo.BeginOutputReadLine();
                mongo.BeginErrorReadLine();
                mongo.WaitForExit();
                if (mongo.ExitCode != 0)
                {
                    throw new DbscException(string.Format("Error running mongo script {0}. Check mongo output above for details.", scriptPath));
                }
            }
        }

        private string GetMongoArgs(MongoUpdateOptions options, string scriptPath)
        {
            // mongo --norc [--port portnum] --host hostname [-u username] [-p password] [--eval jsToEval] dbname [script.js]
            if (scriptPath == null)
            {
                throw new ArgumentNullException("scriptPath");
            }

            List<string> args = new List<string>(11);
            string noRcArg = "--norc";
            args.Add(noRcArg);

            if (options.TargetDatabase.Port != null)
            {
                string portArg = string.Format("--port {0}", options.TargetDatabase.Port.Value.ToString(CultureInfo.InvariantCulture));
                args.Add(portArg);
            }

            string hostArg = string.Format("--host {0}", options.TargetDatabase.Server.QuoteCommandLineArg());
            args.Add(hostArg);

            if (options.TargetDatabase.Username != null)
            {
                string userArg = string.Format("-u {0}", options.TargetDatabase.Username.QuoteCommandLineArg());
                args.Add(userArg);
            }

            if (options.TargetDatabase.Password != null)
            {
                string passwordArg = string.Format("-p {0}", options.TargetDatabase.Password.QuoteCommandLineArg());
                args.Add(passwordArg);
            }

            string dbNameArg = options.TargetDatabase.Database.QuoteCommandLineArg();
            args.Add(dbNameArg);

            string scriptArg = scriptPath.QuoteCommandLineArg();
            args.Add(scriptArg);

            string argsString = string.Join(" ", args);
            return argsString;
        }

        private void UpdateMetadata(MongoUpdateOptions options, int newRevision, DateTime utcTimestamp)
        {
            using (MongoDbscConnection conn = new MongoDbscConnection(options.TargetDatabase))
            {
                List<Tuple<Expression<Func<dbsc_metadata, object>>, object>> updates = new List<Tuple<Expression<Func<dbsc_metadata, object>>, object>>()
                {
                    Tuple.Create<Expression<Func<dbsc_metadata, object>>, object>(metadata => metadata.Version, newRevision),
                    Tuple.Create<Expression<Func<dbsc_metadata, object>>, object>(metadata => metadata.LastChangeUTC, utcTimestamp)
                };
                conn.UpdateAll(MetadataCollectionName, updates);
            }
        }

        protected override void ImportData(MongoUpdateOptions options)
        {
            DbscEngineWithTableImportExtensions.ImportData(this, options);
        }

        public void ImportData(MongoUpdateOptions options, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadataAlreadyEscaped)
        {
            using (MongoDbscConnection conn = new MongoDbscConnection(options.TargetDatabase))
            {
                string clearMessage;
                if (tablesToImportAlreadyEscaped.Count == allTablesExceptMetadataAlreadyEscaped.Count)
                {
                    clearMessage = "Removing all collections";
                }
                else
                {
                    clearMessage = "Removing collections to import";
                }
                
                // Drop collections
                Utils.DoTimedOperation(clearMessage, () =>
                {
                    foreach (string collectionName in tablesToImportAlreadyEscaped)
                    {
                        conn.DropCollection(collectionName);
                    }
                });

                // Import each collection to import
                foreach (string collectionToImport in tablesToImportAlreadyEscaped)
                {
                    conn.ImportCollection(options.ImportOptions.SourceDatabase, options.TargetDatabase, collectionToImport);
                }
            }
        }

        public ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(DbConnectionInfo connectionInfo)
        {
            // No need to escape names because there is no query language to escape them for.

            using (MongoDbscConnection conn = new MongoDbscConnection(connectionInfo))
            {
                ICollection<string> collectionNames = conn.GetCollectionNames();
                collectionNames.Remove(MetadataCollectionName);
                return collectionNames;
            }
        }
    }
}

/*
 Copyright 2014 Greg Najda

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
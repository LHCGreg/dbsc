using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using MongoDB.Driver;
using System.Globalization;
using System.Diagnostics;
using System.Linq.Expressions;

namespace dbsc.Mongo
{
    class MongoDbscEngine : DbscEngine<MongoCheckoutOptions, MongoUpdateOptions>
    {
        protected override string ScriptExtensionWithoutDot { get { return "js"; } }

        protected override bool ImportIsSupported(out string whyNot)
        {
            whyNot = "Import not supported for MongoDB yet.";
            return false;
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
            ; // Nothing to do here, mongo creates databases when you insert something into one
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

        protected override void RunScriptAndUpdateMetadata(MongoUpdateOptions options, string scriptPath, int newRevision, DateTime utcTimestamp)
        {
            RunScript(options, scriptPath, newRevision);
            UpdateMetadata(options, newRevision, utcTimestamp);
        }

        private void RunScript(MongoUpdateOptions options, string scriptPath, int newRevision)
        {
            string mongoArgs = GetMongoArgs(options, scriptPath);
            Process mongo = new Process()
            {
                StartInfo = new ProcessStartInfo("mongo", mongoArgs)
                {
                    ErrorDialog = false,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            mongo.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            using (mongo)
            {
                mongo.Start();
                mongo.BeginOutputReadLine();
                mongo.WaitForExit();
                if (mongo.ExitCode != 0)
                {
                    throw new DbscException(string.Format("Error running mongo script {0}. Check mongo output above for details.", scriptPath));
                }
            }
        }

        private string GetMongoArgs(MongoUpdateOptions options, string scriptPath)
        {
            // mongo --norc [--port portnum] --host hostname [-u username] [-p password] dbname script.js
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

        protected override void ImportData(MongoUpdateOptions options, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadataAlreadyEscaped)
        {
            throw new NotImplementedException();
        }

        protected override ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(DbConnectionInfo connectionInfo)
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
 Copyright 2013 Greg Najda

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
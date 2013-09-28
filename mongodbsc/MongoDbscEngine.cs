using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using MongoDB.Driver;

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
            MongoDbscConnection conn = new MongoDbscConnection(connectionInfo);
            return conn.ContainsCollection(MetadataCollectionName);
        }

        protected override void CreateDatabase(MongoCheckoutOptions options)
        {
            ; // Nothing to do here, mongo creates databases when you insert something into one
        }

        protected override void InitializeDatabase(MongoCheckoutOptions options, string masterDatabaseName)
        {
            MongoDbscConnection conn = new MongoDbscConnection(options.TargetDatabase);
            conn.CreateCollection(MetadataCollectionName);
            dbsc_metadata metadata = new dbsc_metadata(-1, masterDatabaseName, DateTime.UtcNow);
            conn.Upsert(metadata, MetadataCollectionName);
        }

        protected override int GetRevision(DbConnectionInfo connectionInfo)
        {
            MongoDbscConnection conn = new MongoDbscConnection(connectionInfo);
            dbsc_metadata metadata = conn.GetSingleDocument<dbsc_metadata>(MetadataCollectionName);
            return metadata.Version;
        }

        protected override void RunScriptAndUpdateMetadata(MongoUpdateOptions options, string scriptPath, int newRevision, DateTime utcTimestamp)
        {
            throw new NotImplementedException();
        }

        protected override void ImportData(MongoUpdateOptions options, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadataAlreadyEscaped)
        {
            throw new NotImplementedException();
        }

        protected override ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(DbConnectionInfo connectionInfo)
        {
            // No need to escape names because there is no query language to escape them for.

            MongoDbscConnection conn = new MongoDbscConnection(connectionInfo);
            ICollection<string> collectionNames = conn.GetCollectionNames();
            collectionNames.Remove(MetadataCollectionName);
            return collectionNames;
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
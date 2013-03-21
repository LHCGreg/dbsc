using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace dbsc.Core
{
    public abstract class DbscEngine
    {
        public DbscEngine()
        {
            ;
        }

        protected abstract DbConnectionInfo GetSystemDatabaseConnectionInfo(DbConnectionInfo targetDatabase);
        protected abstract IDbscDbConnection OpenConnection(DbConnectionInfo connectionInfo);
        protected abstract string CreateMetadataTableSql { get; }
        protected abstract string MetadataTableName { get; }
        protected abstract string MetadataPropertyNameColumn { get; }
        protected abstract string MetadataPropertyValueColumn { get; }
        protected abstract bool MetaDataTableExists(IDbscDbConnection conn);
        protected abstract void ImportData(IDbscDbConnection targetConn, IDbscDbConnection sourceConn, ICollection<string> tablesToImport, ICollection<string> allTablesExceptMetadata, ImportOptions options, DbConnectionInfo targetConnectionInfo);
        protected abstract ICollection<string> GetTableNamesExceptMetadata(IDbscDbConnection conn);

        public void Checkout(CheckoutOptions options)
        {
            // Process SQL stack
            SqlStack sqlStack = new SqlStack(options.Directory);

            // Default target database name and source database name to the master database name
            if (options.TargetDatabase.Database == null)
            {
                // XXX: A bit hacky to be modifying the input
                options.TargetDatabase.Database = sqlStack.MasterDatabaseName;
            }

            if (options.ImportOptions != null && options.ImportOptions.SourceDatabase.Database == null)
            {
                options.ImportOptions.SourceDatabase.Database = sqlStack.MasterDatabaseName;
            }

            // If revision was specified, verify that there is a script for tha revision
            if (options.Revision != null && !sqlStack.ScriptsByRevision.ContainsKey(options.Revision.Value))
            {
                if (!sqlStack.ScriptsByRevision.ContainsKey(options.Revision.Value))
                {
                    throw new DbscException(string.Format("Cannot update to r{0} because there is no upgrade script for r{0}.", options.Revision.Value));
                }
            }

            if (options.ImportOptions != null)
            {
                // Check that source database was checked out with dbsc
                using (IDbscDbConnection sourceConn = OpenConnection(options.ImportOptions.SourceDatabase))
                {
                    if (!MetaDataTableExists(sourceConn))
                    {
                        throw new DbscException(string.Format("Source database {0} on {1} was not created with dbsc and cannot be imported from.", options.ImportOptions.SourceDatabase.Database, options.ImportOptions.SourceDatabase.Server));
                    }
                }
            }

            CreateDatabase(options.TargetDatabase, options.CreationTemplate);

            using (IDbscDbConnection conn = OpenConnection(options.TargetDatabase))
            {
                InitializeDatabase(conn, sqlStack.MasterDatabaseName);
                UpdateDatabase(conn, sqlStack, options.Revision, options.ImportOptions, options.TargetDatabase);
            }
        }

        private void CreateDatabase(DbConnectionInfo targetDatabase, string creationTemplate)
        {
            DbConnectionInfo masterDatabaseConnectionInfo = GetSystemDatabaseConnectionInfo(targetDatabase);
            using (IDbscDbConnection masterDatabaseConnection = OpenConnection(masterDatabaseConnectionInfo))
            {
                string creationSql = creationTemplate.Replace("$DatabaseName$", targetDatabase.Database);
                Console.WriteLine("Creating database {0} on {1}.", targetDatabase.Database, targetDatabase.Server);
                masterDatabaseConnection.ExecuteSqlScript(creationSql);
                Console.WriteLine("Created database {0} on {1}.", targetDatabase.Database, targetDatabase.Server);
            }
        }

        protected string RevisionPropertyName { get { return "Version"; } }

        private void InitializeDatabase(IDbscDbConnection conn, string masterDatabaseName)
        {
            conn.ExecuteSql(CreateMetadataTableSql);

            Dictionary<string, string> initialProperties = new Dictionary<string, string>()
                {
                    { "MasterDatabaseName", masterDatabaseName },
                    { RevisionPropertyName, "-1" }
                };
            CreateMetadataProperties(conn, initialProperties);
        }

        protected virtual void CreateMetadataProperties(IDbscDbConnection conn, IDictionary<string, string> properties)
        {
            if (properties.Count == 0)
                return;

            StringBuilder sqlBuilder = new StringBuilder(string.Format(@"INSERT INTO {0}
({1}, {2})
VALUES
", MetadataTableName, MetadataPropertyNameColumn, MetadataPropertyValueColumn));
            int propertyNum = 0;
            Dictionary<string, object> sqlParams = new Dictionary<string, object>();
            foreach (string propertyName in properties.Keys)
            {
                if (propertyNum > 0)
                {
                    sqlBuilder.Append(",");
                }
                sqlBuilder.AppendFormat("(@name{0}, @value{0})\n", propertyNum);
                sqlParams["name" + propertyNum.ToString()] = propertyName;
                sqlParams["value" + propertyNum.ToString()] = properties[propertyName];
                propertyNum++;
            }

            string sql = sqlBuilder.ToString();
            conn.ExecuteSql(sql, sqlParams);
        }

        protected virtual void UpdateMetadataProperty(IDbscDbConnection conn, string propertyName, string propertyValue)
        {
            string sql = string.Format(@"UPDATE {0}
SET {1} = @value
WHERE {2} = @name", MetadataTableName, MetadataPropertyValueColumn, MetadataPropertyNameColumn);

            Dictionary<string, object> sqlParams = new Dictionary<string, object>();
            sqlParams["value"] = propertyValue;
            sqlParams["name"] = propertyName;

            conn.ExecuteSql(sql, sqlParams);
        }

        protected virtual string GetMetadataProperty(IDbscDbConnection conn, string propertyName)
        {
            string sql = string.Format(@"SELECT {0} FROM {1}
WHERE {2} = @name", MetadataPropertyValueColumn, MetadataTableName, MetadataPropertyNameColumn);
            Dictionary<string, object> sqlParams = new Dictionary<string, object>() { { "name", propertyName } };

            string propertyValue = conn.Query<string>(sql, sqlParams).FirstOrDefault();

            if (propertyValue == null)
            {
                throw new DbscException(string.Format("No metadata property {0}.", propertyName));
            }
            else
            {
                return propertyValue;
            }
        }

        public void Update(UpdateOptions options)
        {
            SqlStack sqlStack = new SqlStack(options.Directory);

            if (options.TargetDatabase.Database == null)
            {
                // XXX: A bit hacky to be modifying the input
                options.TargetDatabase.Database = sqlStack.MasterDatabaseName;
            }

            if (options.ImportOptions != null && options.ImportOptions.SourceDatabase.Database == null)
            {
                options.ImportOptions.SourceDatabase.Database = sqlStack.MasterDatabaseName;
            }

            using (IDbscDbConnection conn = OpenConnection(options.TargetDatabase))
            {
                if (!MetaDataTableExists(conn))
                {
                    throw new DbscException(string.Format("Target database {0} on {1} was not created with dbsc and cannot be updated.", options.TargetDatabase.Database, options.TargetDatabase.Server));
                }
                UpdateDatabase(conn, sqlStack, options.Revision, options.ImportOptions, options.TargetDatabase);
            }
        }

        private void UpdateDatabase(IDbscDbConnection conn, SqlStack sqlStack, int? revision, ImportOptions importOptions, DbConnectionInfo targetConnectionInfo)
        {
            string versionString = GetMetadataProperty(conn, RevisionPropertyName);
            int versionBeforeUpdate = int.Parse(versionString); // TODO: tryparse
            int currentVersion = versionBeforeUpdate;

            int sourceDatabaseRevision = -1;
            if (importOptions != null)
            {
                using (IDbscDbConnection sourceConn = OpenConnection(importOptions.SourceDatabase))
                {
                    string sourceRevisionString = GetMetadataProperty(sourceConn, RevisionPropertyName);
                    sourceDatabaseRevision = int.Parse(sourceRevisionString); // TODO: tryparse
                }
            }

            IEnumerable<int> revisionsToUpgradeTo = sqlStack.ScriptsByRevision.Keys.OrderBy(r => r).Where(r => r > versionBeforeUpdate);
            if (revision != null)
            {
                if (versionBeforeUpdate > revision.Value)
                {
                    throw new DbscException(string.Format("Cannot update to r{0} because the database is already at r{1}.", revision.Value, versionBeforeUpdate));
                }
                if (!sqlStack.ScriptsByRevision.ContainsKey(revision.Value))
                {
                    throw new DbscException(string.Format("Cannot update to r{0} because there is no upgrade script for r{0}.", revision.Value));
                }
                revisionsToUpgradeTo = revisionsToUpgradeTo.Where(r => r <= revision.Value);
            }

            foreach (int revisionNumber in revisionsToUpgradeTo)
            {
                string upgradeScriptPath = sqlStack.ScriptsByRevision[revisionNumber];
                Console.WriteLine("Updating to r{0}", revisionNumber);

                // Run upgrade script
                string upgradeScriptSql = File.ReadAllText(upgradeScriptPath);
                conn.ExecuteSqlScript(upgradeScriptSql);

                // Update Version metadata
                string newVersionString = revisionNumber.ToString(CultureInfo.InvariantCulture);
                UpdateMetadataProperty(conn, RevisionPropertyName, newVersionString);
                currentVersion = revisionNumber;

                // check for import
                if (importOptions != null && revisionNumber == sourceDatabaseRevision)
                {
                    using (IDbscDbConnection sourceConn = OpenConnection(importOptions.SourceDatabase))
                    {
                        ImportData(conn, sourceConn, importOptions, targetConnectionInfo);
                    }
                }
            }

            // allow importing when "updating" to the revison the database is already at
            if (!revisionsToUpgradeTo.Any() && versionBeforeUpdate == sourceDatabaseRevision && importOptions != null)
            {
                using (IDbscDbConnection sourceConn = OpenConnection(importOptions.SourceDatabase))
                {
                    ImportData(conn, sourceConn, importOptions, targetConnectionInfo);
                }
            }

            Console.WriteLine("At revision {0}", currentVersion);
        }

        private void ImportData(IDbscDbConnection targetConn, IDbscDbConnection sourceConn, ImportOptions importOptions, DbConnectionInfo targetConnectionInfo)
        {
            Console.WriteLine("Beginning import...");
            Stopwatch timer = Stopwatch.StartNew();

            ICollection<string> tablesExceptMetadata = GetTableNamesExceptMetadata(targetConn);

            ICollection<string> tablesToImport;
            if (importOptions.TablesToImport != null)
            {
                tablesToImport = importOptions.TablesToImport;
            }
            else
            {
                tablesToImport = tablesExceptMetadata;
            }

            ImportData(targetConn, sourceConn, tablesToImport, tablesExceptMetadata, importOptions, targetConnectionInfo);

            timer.Stop();
            Console.WriteLine("Import complete! Took {0}", timer.Elapsed);
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
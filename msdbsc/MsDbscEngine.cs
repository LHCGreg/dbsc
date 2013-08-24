using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Dapper;
using dbsc.Core;
using System.Diagnostics;

namespace dbsc.SqlServer
{
    class MsDbscEngine : DbscEngine
    {
        protected override DbConnectionInfo GetSystemDatabaseConnectionInfo(DbConnectionInfo targetDatabase)
        {
            DbConnectionInfo systemInfo = targetDatabase.Clone();
            systemInfo.Database = "master";
            return systemInfo;
        }

        protected override IDbscDbConnection OpenConnection(DbConnectionInfo connectionInfo)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.ApplicationName = "msdbsc";
            builder.DataSource = connectionInfo.Server;
            builder.InitialCatalog = connectionInfo.Database;

            if (connectionInfo.Username == null)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.UserID = connectionInfo.Username;
                builder.Password = connectionInfo.Password;
            }

            builder.MultipleActiveResultSets = false;

            string connectionString = builder.ToString();

            return new MsDbscDbConnection(connectionString);
        }

        protected override string CreateMetadataTableSql
        {
            get
            {
                return
@"CREATE TABLE dbsc_metadata
(
    property_name nvarchar(256) NOT NULL PRIMARY KEY,
    property_value nvarchar(max)
)";
            }
        }

        protected override string MetadataTableName { get { return "dbsc_metadata"; } }
        protected override string MetadataPropertyNameColumn { get { return "property_name"; } }
        protected override string MetadataPropertyValueColumn { get { return "property_value"; } }

        protected override bool MetaDataTableExists(IDbscDbConnection conn)
        {
            SqlConnection sqlConn = ((MsDbscDbConnection)conn).Connection;
            string sql = @"SELECT count(*) FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME = 'dbsc_metadata'";
            return sqlConn.Query<int>(sql).First() > 0;
        }

        private string QuoteSqlServerIdentifier(string identifier)
        {
            // Replace ] with ]] and enclose in []
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        private string QuoteSqlServerIdentifier(string schema, string identifier)
        {
            return QuoteSqlServerIdentifier(schema) + "." + QuoteSqlServerIdentifier(identifier);
        }

        private class Table
        {
            public string TableSchema { get; set; }
            public string TableName { get; set; }
        }

        private class Index
        {
            public string IndexName { get; set; }
            public string TableSchema { get; set; }
            public string TableName { get; set; }
        }

        private class RecoveryModel
        {
            public int RecoveryModelId { get; set; }
        }

        protected override ICollection<string> GetTableNamesExceptMetadata(IDbscDbConnection conn)
        {
            SqlConnection sqlConn = ((MsDbscDbConnection)conn).Connection;
            string sql = @"SELECT TABLE_SCHEMA AS TableSchema, TABLE_NAME AS TableName FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME <> 'dbsc_metadata'";
            List<string> tables = sqlConn.Query<Table>(sql).Select(t => QuoteSqlServerIdentifier(t.TableSchema, t.TableName)).ToList();
            return tables;
        }

        protected override void ImportData(IDbscDbConnection targetConn, IDbscDbConnection sourceConn, ICollection<string> tablesToImport, ICollection<string> allTablesExceptMetadata, ImportOptions options, DbConnectionInfo targetConnectionInfo)
        {
            SqlConnection sqlServerTargetConn = ((MsDbscDbConnection)targetConn).Connection;
            SqlConnection sqlServerSourceConn = ((MsDbscDbConnection)sourceConn).Connection;

            // Disable constraints
            Console.Write("Removing constraints...");
            try
            {
                Stopwatch removeConstraintsTimer = Stopwatch.StartNew();

                foreach (string table in allTablesExceptMetadata)
                {
                    string disableConstraintsSql = string.Format("ALTER TABLE {0} NOCHECK CONSTRAINT ALL", table);
                    sqlServerTargetConn.Execute(disableConstraintsSql);
                }

                removeConstraintsTimer.Stop();
                Console.Write(removeConstraintsTimer.Elapsed);
            }
            finally
            {
                Console.WriteLine();
            }

            // Disable indexes
            List<Index> nonClusteredIndexes = new List<Index>();
            Console.Write("Disabling non-clustered indexes...");
            try
            {
                Stopwatch removeIndexesTimer = Stopwatch.StartNew();

                string indexQuerySql =
@"SELECT Ind.name AS IndexName, TAB.TABLE_SCHEMA AS TableSchema, Tab.TABLE_NAME AS TableName FROM sys.indexes Ind
JOIN sys.objects Obj ON Ind.object_id = Obj.object_id
JOIN sys.schemas Sch ON Obj.schema_id = Sch.schema_id
JOIN INFORMATION_SCHEMA.TABLES AS Tab ON Obj.name = Tab.TABLE_NAME AND Sch.name = Tab.TABLE_SCHEMA -- Only indexes on normal tables, not system tables
WHERE Ind.type <> 1 -- No clustered indexes
AND Ind.name IS NOT NULL -- Tables without a primary key clustered index are heaps and have an index with a null name";

                nonClusteredIndexes = sqlServerTargetConn.Query<Index>(indexQuerySql).ToList();
                foreach (Index index in nonClusteredIndexes)
                {
                    string disableIndexSql = string.Format("ALTER INDEX {0} ON {1} DISABLE", QuoteSqlServerIdentifier(index.IndexName), QuoteSqlServerIdentifier(index.TableSchema, index.TableName));
                    sqlServerTargetConn.Execute(disableIndexSql);
                }

                removeIndexesTimer.Stop();
                Console.Write(removeIndexesTimer.Elapsed);
            }
            finally
            {
                Console.WriteLine();
            }

            Console.Write("Clearing all tables...");
            try
            {
                Stopwatch clearTablesTimer = Stopwatch.StartNew();

                foreach (string table in allTablesExceptMetadata)
                {
                    //string truncateSql = string.Format("TRUNCATE TABLE {0}", table);
                    // Can't use TRUNCATE if there's a foreign key to the table, even if the FK constraint is disabled.
                    // Possible future improvement would be to drop the FK constraints and recreate them after import instead of disabling them
                    string truncateSql = string.Format("DELETE FROM {0}", table); // table is already bracketed and schema-qualified
                    sqlServerTargetConn.Execute(truncateSql);
                }

                clearTablesTimer.Stop();
                Console.Write(clearTablesTimer.Elapsed);
            }
            finally
            {
                Console.WriteLine();
            }

            // If recovery model is full, switch to bulk-logged recovery model for import and switch back after import
            string recoveryModelQuerySql = "SELECT recovery_model AS RecoveryModelId FROM sys.databases WHERE name = @dbName";
            RecoveryModel recoveryModel = sqlServerTargetConn.Query<RecoveryModel>(recoveryModelQuerySql, param: new { dbName = targetConnectionInfo.Database }).First();
            int recoveryModelId = recoveryModel.RecoveryModelId;
            
            // 1 = FULL, 2 = BULK_LOGGED, 3 = SIMPLE
            bool recoveryModelWasFullBeforeImport = recoveryModelId == 1;
            if(recoveryModelWasFullBeforeImport)
            {
                Console.WriteLine("Setting recovery model to BULK_LOGGED...");
                string setRecoveryToBulkLoggedSql = string.Format("ALTER DATABASE {0} SET RECOVERY BULK_LOGGED", QuoteSqlServerIdentifier(targetConnectionInfo.Database));
                sqlServerTargetConn.Execute(setRecoveryToBulkLoggedSql);
            }

            // table is already schema-qualified and enclosed in []
            foreach (string table in tablesToImport)
            {
                Console.Write("Importing {0}...", table);
                try
                {
                    Stopwatch importTimer = Stopwatch.StartNew();

                    SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlServerTargetConn, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.TableLock, externalTransaction: null);
                    bulkCopy.BulkCopyTimeout = int.MaxValue;
                    bulkCopy.DestinationTableName = table;

                    string importSql = string.Format("SELECT * FROM {0}", table);
                    using (SqlCommand importQuery = new SqlCommand(importSql, sqlServerSourceConn))
                    {
                        using (SqlDataReader reader = importQuery.ExecuteReader())
                        {
                            bulkCopy.WriteToServer(reader);
                        }
                    }
                    
                    importTimer.Stop();
                    Console.Write(importTimer.Elapsed);
                }
                finally
                {
                    Console.WriteLine();
                }
            }

            // If recovery model was full before import and set to bulk-logged for the import, set it back to full
            if(recoveryModelWasFullBeforeImport)
            {
                Console.WriteLine("Setting recovery model back to FULL...");
                string setRecoveryToFullSql = string.Format("ALTER DATABASE {0} SET RECOVERY FULL", QuoteSqlServerIdentifier(targetConnectionInfo.Database));
                sqlServerTargetConn.Execute(setRecoveryToFullSql);
            }

            // Enable indexes that were disabled
            Console.Write("Enabling and rebuilding non-clustered indexes...");
            try
            {
                Stopwatch rebuildIndexTimer = Stopwatch.StartNew();
                foreach (Index index in nonClusteredIndexes)
                {
                    string disableIndexSql = string.Format("ALTER INDEX {0} ON {1} REBUILD", QuoteSqlServerIdentifier(index.IndexName), QuoteSqlServerIdentifier(index.TableSchema, index.TableName));
                    sqlServerTargetConn.Execute(disableIndexSql);
                }

                rebuildIndexTimer.Stop();
                Console.Write(rebuildIndexTimer.Elapsed);
            }
            finally
            {
                Console.WriteLine();
            }

            // Enable constraints
            Console.Write("Enabling constraints...");
            try
            {
                Stopwatch enableConstraintsTimer = Stopwatch.StartNew();

                foreach (string table in allTablesExceptMetadata)
                {
                    string enableConstraintsSql = string.Format("ALTER TABLE {0} WITH CHECK CHECK CONSTRAINT ALL", table);
                    sqlServerTargetConn.Execute(enableConstraintsSql);
                }

                enableConstraintsTimer.Stop();
                Console.Write(enableConstraintsTimer.Elapsed);
            }
            finally
            {
                Console.WriteLine();
            }

            // And we're done
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
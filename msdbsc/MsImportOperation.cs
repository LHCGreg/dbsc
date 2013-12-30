using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using System.Diagnostics;
using System.Data.SqlClient;

namespace dbsc.SqlServer
{
    class MsImportOperation
    {
        private SqlUpdateOptions m_options;
        private ICollection<string> m_tablesToImportAlreadyEscaped;
        private ICollection<string> m_allTablesExceptMetadataAlreadyEscaped;

        private bool m_recoveryModelWasFullBeforeImport;
        private List<Index> m_nonClusteredIndexes;

        const int truncateTimeoutInSeconds = 60 * 60;
        const int enableIndexTimeoutInSeconds = 60 * 60 * 6;
        const int enableConstraintsTimeoutInSeconds = 60 * 60 * 6;

        public MsImportOperation(SqlUpdateOptions options, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadataAlreadyEscaped)
        {
            m_options = options;
            m_tablesToImportAlreadyEscaped = tablesToImportAlreadyEscaped;
            m_allTablesExceptMetadataAlreadyEscaped = allTablesExceptMetadataAlreadyEscaped;
        }

        public void Run()
        {
            using (MsDbscDbConnection targetConn = new MsDbscDbConnection(m_options.TargetDatabase))
            {
                using (MsDbscDbConnection sourceConn = new MsDbscDbConnection(m_options.ImportOptions.SourceDatabase))
                {
                    PrepareTargetForImport(targetConn);
                    DoImport(targetConn, sourceConn);
                }

                DoPostImport(targetConn);
            }
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

        private class SourceDbInfo
        {
            public int snapshot_isolation_state { get; set; }
        }

        private void PrepareTargetForImport(MsDbscDbConnection targetConn)
        {
            // Disable constraints
            Utils.DoTimedOperation("Removing constraints", () =>
            {
                foreach (string table in m_allTablesExceptMetadataAlreadyEscaped)
                {
                    string disableConstraintsSql = string.Format("ALTER TABLE {0} NOCHECK CONSTRAINT ALL", table);
                    targetConn.ExecuteSql(disableConstraintsSql);
                }
            });

            // Disable indexes
            m_nonClusteredIndexes = new List<Index>();
            Utils.DoTimedOperation("Disabling non-clustered indexes", () =>
            {
                string indexQuerySql =
@"SELECT Ind.name AS IndexName, TAB.TABLE_SCHEMA AS TableSchema, Tab.TABLE_NAME AS TableName FROM sys.indexes Ind
JOIN sys.objects Obj ON Ind.object_id = Obj.object_id
JOIN sys.schemas Sch ON Obj.schema_id = Sch.schema_id
JOIN INFORMATION_SCHEMA.TABLES AS Tab ON Obj.name = Tab.TABLE_NAME AND Sch.name = Tab.TABLE_SCHEMA -- Only indexes on normal tables, not system tables
WHERE Ind.type <> 1 -- No clustered indexes
AND Ind.name IS NOT NULL -- Tables without a primary key clustered index are heaps and have an index with a null name";

                m_nonClusteredIndexes = targetConn.Query<Index>(indexQuerySql).ToList();
                foreach (Index index in m_nonClusteredIndexes)
                {
                    string disableIndexSql = string.Format("ALTER INDEX {0} ON {1} DISABLE",
                        MsDbscDbConnection.QuoteSqlServerIdentifier(index.IndexName), MsDbscDbConnection.QuoteSqlServerIdentifier(index.TableSchema, index.TableName));
                    targetConn.ExecuteSql(disableIndexSql);
                }
            });

            Utils.DoTimedOperation("Clearing all tables", () =>
            {
                foreach (string table in m_allTablesExceptMetadataAlreadyEscaped)
                {
                    //string truncateSql = string.Format("TRUNCATE TABLE {0}", table);
                    // Can't use TRUNCATE if there's a foreign key to the table, even if the FK constraint is disabled.
                    // Possible future improvement would be to drop the FK constraints and recreate them after import instead of disabling them
                    string truncateSql = string.Format("DELETE FROM {0}", table);
                    targetConn.ExecuteSql(truncateSql, timeoutInSeconds: truncateTimeoutInSeconds);
                }
            });

            // If recovery model is full, switch to bulk-logged recovery model for import and switch back after import
            string recoveryModelQuerySql = "SELECT recovery_model AS RecoveryModelId FROM sys.databases WHERE name = @dbName";
            var recoveryModelQueryParams = new Dictionary<string, object>() { { "dbName", m_options.TargetDatabase.Database } };
            RecoveryModel recoveryModel = targetConn.Query<RecoveryModel>(recoveryModelQuerySql, recoveryModelQueryParams).First();
            int recoveryModelId = recoveryModel.RecoveryModelId;

            // 1 = FULL, 2 = BULK_LOGGED, 3 = SIMPLE
            m_recoveryModelWasFullBeforeImport = recoveryModelId == 1;
            if (m_recoveryModelWasFullBeforeImport)
            {
                Console.WriteLine("Setting recovery model to BULK_LOGGED...");
                string setRecoveryToBulkLoggedSql = string.Format("ALTER DATABASE {0} SET RECOVERY BULK_LOGGED",
                    MsDbscDbConnection.QuoteSqlServerIdentifier(m_options.TargetDatabase.Database));
                targetConn.ExecuteSql(setRecoveryToBulkLoggedSql);
            }
        }

        private void DoImport(MsDbscDbConnection targetConn, MsDbscDbConnection sourceConn)
        {
            string sourceDbInfoSql = "SELECT snapshot_isolation_state FROM sys.databases WHERE name = @dbName";
            var sourceDbInfoParams = new Dictionary<string, object>() { { "dbName", m_options.ImportOptions.SourceDatabase.Database } };
            SourceDbInfo sourceDbInfo = sourceConn.Query<SourceDbInfo>(sourceDbInfoSql, sourceDbInfoParams).FirstOrDefault();
            if (sourceDbInfo == null)
            {
                throw new DbscException("No rows returned when querying source database info.");
            }

            // Use a snapshot isolation transaction if snapshot isolation is enabled.
            // This will allow doing an import of a live, in-use database while getting transactionally consistent data.
            // If snapshot isolation is not enabled, just pull data without a transaction. If the database is in use,
            // you may get transactionally inconsistent data.
            SqlTransaction sourceDbTransaction = null;
            if (sourceDbInfo.snapshot_isolation_state == 1)
            {
                sourceDbTransaction = sourceConn.BeginSnapshotTransaction();
            }

            // using (null) is ok
            using (sourceDbTransaction)
            {
                foreach (string table in m_tablesToImportAlreadyEscaped)
                {
                    Utils.DoTimedOperation(string.Format("Importing {0}", table), () =>
                    {
                        targetConn.ImportTable(sourceConn, table, sourceDbTransaction);
                    });
                }

                // Finish the transaction on the source database.
                // Didn't write anything to source database, so a commit would function as well, but we definitely don't want
                // to write anything to the source database.
                if (sourceDbTransaction != null)
                {
                    sourceDbTransaction.Rollback();
                }
            }
        }

        private void DoPostImport(MsDbscDbConnection targetConn)
        {
            // If recovery model was full before import and set to bulk-logged for the import, set it back to full
            if (m_recoveryModelWasFullBeforeImport)
            {
                Console.WriteLine("Setting recovery model back to FULL...");
                string setRecoveryToFullSql = string.Format("ALTER DATABASE {0} SET RECOVERY FULL",
                    MsDbscDbConnection.QuoteSqlServerIdentifier(m_options.TargetDatabase.Database));
                targetConn.ExecuteSql(setRecoveryToFullSql);
            }

            // Enable indexes that were disabled
            Utils.DoTimedOperation("Enabling and rebuilding non-clustered indexes", () =>
            {
                foreach (Index index in m_nonClusteredIndexes)
                {
                    string enableIndexSql = string.Format("ALTER INDEX {0} ON {1} REBUILD",
                        MsDbscDbConnection.QuoteSqlServerIdentifier(index.IndexName), MsDbscDbConnection.QuoteSqlServerIdentifier(index.TableSchema, index.TableName));
                    targetConn.ExecuteSql(enableIndexSql, timeoutInSeconds: enableIndexTimeoutInSeconds);
                }
            });

            // Enable constraints
            Utils.DoTimedOperation("Enabling constraints", () =>
            {
                foreach (string table in m_allTablesExceptMetadataAlreadyEscaped)
                {
                    string enableConstraintsSql = string.Format("ALTER TABLE {0} WITH CHECK CHECK CONSTRAINT ALL", table);
                    targetConn.ExecuteSql(enableConstraintsSql, timeoutInSeconds: enableConstraintsTimeoutInSeconds);
                }
            });
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
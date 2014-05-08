using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Npgsql;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.Postgres
{
    // Moved this out to a class to break up the three import stages without having to pass state like fkCreationSql around
    class PgImportOperation
    {
        private SqlUpdateOptions _options;
        private ICollection<string> _tablesToImportAlreadyEscaped;
        private ICollection<string> _allTablesExceptMetadataAlreadyEscaped;

        private List<string> _fkCreationSql;
        private List<string> _pkCreationSql;
        private List<string> _indexCreationSql;

        const int EnableIndexTimeoutInSeconds = 60 * 60 * 6;
        const int EnableConstraintTimeoutInSeconds = 60 * 60 * 6;
        const int VacuumTimeoutInSeconds = 60 * 60 * 6;

        public PgImportOperation(SqlUpdateOptions options, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadataAlreadyEscaped)
        {
            _options = options;
            _tablesToImportAlreadyEscaped = tablesToImportAlreadyEscaped;
            _allTablesExceptMetadataAlreadyEscaped = allTablesExceptMetadataAlreadyEscaped;
        }

        private class ConstraintInfo
        {
            public string conname { get; set; }
            public char contype { get; set; }
            public string nspname { get; set; }
            public string tablename { get; set; }
            public string def { get; set; }
        }

        private class IndexInfo
        {
            public string index_schema { get; set; }
            public string index_name { get; set; }
            public string def { get; set; }
        }

        public void Run()
        {
            using (PgDbscDbConnection targetConn = new PgDbscDbConnection(_options.TargetDatabase))
            {
                using (NpgsqlTransaction targetTransaction = targetConn.BeginTransaction())
                {
                    using (PgDbscDbConnection sourceConn = new PgDbscDbConnection(_options.ImportOptions.SourceDatabase))
                    {
                        PrepareTargetForImport(targetConn, targetTransaction);
                        DoImport(targetConn, targetTransaction, sourceConn);
                    }

                    DoPostImport(targetConn, targetTransaction);

                    Utils.DoTimedOperation("Committing transaction", () =>
                    {
                        targetTransaction.Commit();
                    });
                }

                Utils.DoTimedOperation("Vacuuming", () =>
                {
                    targetConn.ExecuteSql("VACUUM ANALYZE", timeoutInSeconds: VacuumTimeoutInSeconds);
                });
            }
        }

        private void PrepareTargetForImport(PgDbscDbConnection targetConn, NpgsqlTransaction targetTransaction)
        {
            // Disable foreign key constraints and primary key constraints temporarily by removing them, then recreating them after the import

            _fkCreationSql = new List<string>();
            _pkCreationSql = new List<string>();

            Utils.DoTimedOperation("Removing foreign key and primary key constraints", () =>
            {
                string keySql = @"SELECT pg_constraint.conname, pg_constraint.contype, pg_namespace.nspname, pg_class.relname AS tablename, pg_get_constraintdef(pg_constraint.oid) AS def
FROM pg_constraint
JOIN pg_class ON pg_constraint.conrelid = pg_class.oid
JOIN pg_namespace ON pg_class.relnamespace = pg_namespace.oid
WHERE (pg_constraint.contype = 'f' OR pg_constraint.contype = 'p')
AND pg_namespace.nspname NOT LIKE 'pg_%' AND pg_namespace.nspname <> 'information_schema'";

                List<ConstraintInfo> keys = targetConn.Query<ConstraintInfo>(keySql, targetTransaction).ToList();

                // Drop foreign keys first because they depend on primary keys
                foreach (ConstraintInfo key in keys.OrderBy(k => k.contype == 'f' ? 1 : 2))
                {
                    string qualifiedTableName = PgDbscDbConnection.QuotePgIdentifier(key.nspname, key.tablename);
                    string quotedConstraintName = PgDbscDbConnection.QuotePgIdentifier(key.conname);
                    string dropSql = string.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", qualifiedTableName, quotedConstraintName);
                    targetConn.ExecuteSql(dropSql, targetTransaction);
                    string createSql = string.Format("ALTER TABLE {0} ADD {1}", qualifiedTableName, key.def);
                    if (key.contype == 'f')
                        _fkCreationSql.Add(createSql);
                    else
                        _pkCreationSql.Add(createSql);
                }
            });

            // Remove indexes, then recreate them when done importing
            _indexCreationSql = new List<string>();
            Utils.DoTimedOperation("Removing indexes", () =>
            {
                string indexSql = @"
SELECT ind_schema.nspname AS index_schema, ind_more.relname AS index_name, pg_get_indexdef(ind.indexrelid) AS def FROM pg_index AS ind
JOIN pg_class AS tab ON ind.indrelid = tab.oid
JOIN pg_class AS ind_more ON ind.indexrelid = ind_more.oid
JOIN pg_namespace AS table_schema ON tab.relnamespace = table_schema.oid
JOIN pg_namespace AS ind_schema ON ind_more.relnamespace = ind_schema.oid
WHERE table_schema.nspname NOT LIKE 'pg_%' AND table_schema.nspname <> 'information_schema'
AND ind_schema.nspname NOT LIKE 'pg_%' AND ind_schema.nspname <> 'information_schema'
AND tab.relname <> 'dbsc_metadata'";

                List<IndexInfo> indexes = targetConn.Query<IndexInfo>(indexSql, targetTransaction).ToList();
                foreach (IndexInfo index in indexes)
                {
                    string qualifiedIndexName = PgDbscDbConnection.QuotePgIdentifier(index.index_schema, index.index_name);
                    string dropSql = string.Format("DROP INDEX {0}", qualifiedIndexName);
                    targetConn.ExecuteSql(dropSql, targetTransaction);
                    _indexCreationSql.Add(index.def);
                }
            });

            string clearMessage;
            if (_tablesToImportAlreadyEscaped.Count == _allTablesExceptMetadataAlreadyEscaped.Count)
            {
                clearMessage = "Clearing all tables";
            }
            else
            {
                clearMessage = "Clearing tables to import";
            }

            Utils.DoTimedOperation(clearMessage, () =>
            {
                foreach (string table in _tablesToImportAlreadyEscaped)
                {
                    string clearTableSql = string.Format("TRUNCATE TABLE {0}", table);
                    targetConn.ExecuteSql(clearTableSql, targetTransaction);
                }
            });
        }

        private void DoImport(PgDbscDbConnection targetConn, NpgsqlTransaction targetTransaction, PgDbscDbConnection sourceConn)
        {
            using (NpgsqlTransaction sourceTransaction = sourceConn.BeginTransaction())
            {
                foreach (string table in _tablesToImportAlreadyEscaped)
                {
                    Utils.DoTimedOperation(string.Format("Importing {0}", table), () =>
                    {
                        targetConn.ImportTable(sourceConn, table, targetDbTransaction: targetTransaction, sourceDbTransaction: sourceTransaction);
                    });
                }

                // Finish the transaction on the source database.
                // Didn't write anything to source database, so a commit would function as well, but we definitely don't want
                // to write anything to the source database.
                sourceTransaction.Rollback();
            }
        }

        private void DoPostImport(PgDbscDbConnection targetConn, NpgsqlTransaction targetTransaction)
        {
            // Add the indexes back
            Utils.DoTimedOperation("Adding indexes back", () =>
            {
                foreach (string sql in _indexCreationSql)
                {
                    targetConn.ExecuteSql(sql, targetTransaction, timeoutInSeconds: EnableIndexTimeoutInSeconds);
                }
            });

            // Add the foreign key and primary key constraints back
            Utils.DoTimedOperation("Adding foreign key and primary key constraints back", () =>
            {
                // Create primary keys before foreign keys because the foreign keys depend on the primary keys.
                foreach (string sql in _pkCreationSql)
                {
                    targetConn.ExecuteSql(sql, targetTransaction, timeoutInSeconds: EnableConstraintTimeoutInSeconds);
                }
                foreach (string sql in _fkCreationSql)
                {
                    targetConn.ExecuteSql(sql, targetTransaction, timeoutInSeconds: EnableConstraintTimeoutInSeconds);
                }
            });
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
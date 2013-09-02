using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using Dapper;
using dbsc.Core;
using MiscUtil.IO;
using System.Diagnostics;

namespace dbsc.Postgres
{
    class PgDbscEngine : DbscEngine<PgDbscDbConnection>
    {
        public PgDbscEngine()
        {
            ;
        }

        protected override DbConnectionInfo GetSystemDatabaseConnectionInfo(DbConnectionInfo targetDatabase)
        {
            DbConnectionInfo postgresDbInfo = targetDatabase.Clone();
            postgresDbInfo.Database = "postgres";
            return postgresDbInfo;
        }

        protected override PgDbscDbConnection OpenConnection(DbConnectionInfo connectionInfo)
        {
            return new PgDbscDbConnection(connectionInfo);
        }

        protected override string CreateMetadataTableSql
        {
            get
            {
                return
@"CREATE TABLE dbsc_metadata
(
    property_name text NOT NULL PRIMARY KEY,
    property_value text
)";
            }
        }

        protected override string MetadataTableName { get { return "dbsc_metadata"; } }
        protected override string MetadataPropertyNameColumn { get { return "property_name"; } }
        protected override string MetadataPropertyValueColumn { get { return "property_value"; } }

        protected override ICollection<string> GetTableNamesExceptMetadata(PgDbscDbConnection conn)
        {
            string sql = @"SELECT table_schema, table_name FROM information_schema.tables
WHERE table_schema NOT LIKE 'pg_%' AND table_schema <> 'information_schema'
AND table_type = 'BASE TABLE'
AND table_name <> 'dbsc_metadata'";

            List<string> tables = conn.Query<Table>(sql).Select(t => QuotePgIdentifier(t.table_schema, t.table_name)).ToList();
            return tables;
        }

        protected override bool MetaDataTableExists(PgDbscDbConnection conn)
        {
            string sql = @"SELECT count(*) FROM information_schema.tables
WHERE table_type = 'BASE TABLE'
AND table_name = 'dbsc_metadata'";
            return conn.Query<long>(sql).First() > 0;
        }

        private string QuotePgIdentifier(string identifier)
        {
            // Replace quotes with quotequote and enclose in quotes
            return "\"" + identifier.Replace("\"", "\"\"") + "\"";
        }

        private string QuotePgIdentifier(string schema, string identifier)
        {
            return QuotePgIdentifier(schema) + "." + QuotePgIdentifier(identifier);
        }

        private class Table
        {
            public string table_schema { get; set; }
            public string table_name { get; set; }
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

        protected override void ImportData(PgDbscDbConnection targetConn, PgDbscDbConnection sourceConn, ICollection<string> tablesToImport, ICollection<string> allTablesExceptMetadata, ImportOptions options, DbConnectionInfo targetConnectionInfo)
        {
            const int enableIndexTimeoutInSeconds = 60 * 60 * 6;
            const int enableConstraintTimeoutInSeconds = 60 * 60 * 6;
            const int vacuumTimeoutInSeconds = 60 * 60 * 6;

            using (NpgsqlTransaction transaction = targetConn.BeginTransaction())
            {
                // Disable foreign key constraints and primary key constraints temporarily by removing them, then recreating them after the import

                Console.Write("Removing foreign key and primary key constraints...");
                List<string> fkCreationSql = new List<string>();
                List<string> pkCreationSql = new List<string>();
                try
                {
                    Stopwatch keyTimer = Stopwatch.StartNew();
                    string keySql = @"SELECT pg_constraint.conname, pg_constraint.contype, pg_namespace.nspname, pg_class.relname AS tablename, pg_get_constraintdef(pg_constraint.oid) AS def
FROM pg_constraint
JOIN pg_class ON pg_constraint.conrelid = pg_class.oid
JOIN pg_namespace ON pg_class.relnamespace = pg_namespace.oid
WHERE (pg_constraint.contype = 'f' OR pg_constraint.contype = 'p')
AND pg_namespace.nspname NOT LIKE 'pg_%' AND pg_namespace.nspname <> 'information_schema'";

                    List<ConstraintInfo> keys = targetConn.Query<ConstraintInfo>(keySql, transaction).ToList();

                    // Drop foreign keys first because they depend on primary keys
                    foreach (ConstraintInfo key in keys.OrderBy(k => k.contype == 'f' ? 1 : 2))
                    {
                        string qualifiedTableName = QuotePgIdentifier(key.nspname, key.tablename);
                        string quotedConstraintName = QuotePgIdentifier(key.conname);
                        string dropSql = string.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", qualifiedTableName, quotedConstraintName);
                        targetConn.ExecuteSql(dropSql, transaction);
                        string createSql = string.Format("ALTER TABLE {0} ADD {1}", qualifiedTableName, key.def);
                        if (key.contype == 'f')
                            fkCreationSql.Add(createSql);
                        else
                            pkCreationSql.Add(createSql);
                    }

                    keyTimer.Stop();
                    Console.Write(keyTimer.Elapsed);
                }
                finally
                {
                    Console.WriteLine();
                }

                // Remove indexes, then recreate them when done importing
                Console.Write("Removing indexes...");
                List<string> indexCreationSql = new List<string>();
                try
                {
                    Stopwatch indexTimer = Stopwatch.StartNew();
                    string indexSql = @"
SELECT ind_schema.nspname AS index_schema, ind_more.relname AS index_name, pg_get_indexdef(ind.indexrelid) AS def FROM pg_index AS ind
JOIN pg_class AS tab ON ind.indrelid = tab.oid
JOIN pg_class AS ind_more ON ind.indexrelid = ind_more.oid
JOIN pg_namespace AS table_schema ON tab.relnamespace = table_schema.oid
JOIN pg_namespace AS ind_schema ON ind_more.relnamespace = ind_schema.oid
WHERE table_schema.nspname NOT LIKE 'pg_%' AND table_schema.nspname <> 'information_schema'
AND ind_schema.nspname NOT LIKE 'pg_%' AND ind_schema.nspname <> 'information_schema'
AND tab.relname <> 'dbsc_metadata'";

                    List<IndexInfo> indexes = targetConn.Query<IndexInfo>(indexSql, transaction).ToList();
                    foreach (IndexInfo index in indexes)
                    {
                        string qualifiedIndexName = QuotePgIdentifier(index.index_schema, index.index_name);
                        string dropSql = string.Format("DROP INDEX {0}", qualifiedIndexName);
                        targetConn.ExecuteSql(dropSql, transaction);
                        indexCreationSql.Add(index.def);
                    }

                    indexTimer.Stop();
                    Console.Write(indexTimer.Elapsed);
                }
                finally
                {
                    Console.WriteLine();
                }

                Console.Write("Clearing all tables...");
                try
                {
                    Stopwatch clearTableTimer = Stopwatch.StartNew();
                    foreach (string table in allTablesExceptMetadata)
                    {
                        string clearTableSql = string.Format("TRUNCATE TABLE {0}", table);
                        targetConn.ExecuteSql(clearTableSql, transaction);
                    }
                    clearTableTimer.Stop();
                    Console.Write(clearTableTimer.Elapsed);
                }
                finally
                {
                    Console.WriteLine();
                }

                foreach (string table in tablesToImport)
                {
                    Console.Write("Importing {0}...", table);
                    try
                    {
                        Stopwatch timer = Stopwatch.StartNew();

                        targetConn.ImportTable(sourceConn, table, targetDbTransaction: transaction);

                        timer.Stop();
                        Console.Write(timer.Elapsed);
                    }
                    finally
                    {
                        Console.WriteLine();
                    }
                }

                // Add the indexes back
                Console.Write("Adding indexes back...");
                try
                {
                    Stopwatch indexTimer = Stopwatch.StartNew();
                    foreach (string sql in indexCreationSql)
                    {
                        targetConn.ExecuteSql(sql, transaction, timeoutInSeconds: enableIndexTimeoutInSeconds);
                    }
                    indexTimer.Stop();
                    Console.Write(indexTimer.Elapsed);
                }
                finally
                {
                    Console.WriteLine();
                }

                // Add the foreign key and primary key constraints back
                Console.Write("Adding foreign key and primary key constraints back...");
                try
                {
                    Stopwatch fkTimer = Stopwatch.StartNew();
                    // Create primary keys before foreign keys because the foreign keys depend on the primary keys.
                    foreach (string sql in pkCreationSql)
                    {
                        targetConn.ExecuteSql(sql, transaction, timeoutInSeconds: enableConstraintTimeoutInSeconds);
                    }
                    foreach (string sql in fkCreationSql)
                    {
                        targetConn.ExecuteSql(sql, transaction, timeoutInSeconds: enableConstraintTimeoutInSeconds);
                    }
                    fkTimer.Stop();
                    Console.Write(fkTimer.Elapsed);
                }
                finally
                {
                    Console.WriteLine();
                }

                Console.Write("Committing transaction...");
                try
                {
                    Stopwatch timer = Stopwatch.StartNew();
                    transaction.Commit();
                    timer.Stop();
                    Console.Write(timer.Elapsed);
                }
                finally
                {
                    Console.WriteLine();
                }
            }

            Console.Write("Vacuuming...");
            try
            {
                Stopwatch timer = Stopwatch.StartNew();
                targetConn.ExecuteSql("VACUUM ANALYZE", timeoutInSeconds: vacuumTimeoutInSeconds);
                timer.Stop();
                Console.Write(timer.Elapsed);
            }
            finally
            {
                Console.WriteLine();
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
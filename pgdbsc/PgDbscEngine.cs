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
    class PgDbscEngine : SqlDbscEngine<SqlCheckoutOptions, SqlUpdateOptions, PgDbscDbConnection>
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

        private class Table
        {
            public string table_schema { get; set; }
            public string table_name { get; set; }
        }

        protected override ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(PgDbscDbConnection conn)
        {
            string sql = @"SELECT table_schema, table_name FROM information_schema.tables
WHERE table_schema NOT LIKE 'pg_%' AND table_schema <> 'information_schema'
AND table_type = 'BASE TABLE'
AND table_name <> 'dbsc_metadata'";

            List<string> tables = conn.Query<Table>(sql).Select(t => PgDbscDbConnection.QuotePgIdentifier(t.table_schema, t.table_name)).ToList();
            return tables;
        }

        protected override bool MetadataTableExists(PgDbscDbConnection conn)
        {
            string sql = @"SELECT count(*) FROM information_schema.tables
WHERE table_type = 'BASE TABLE'
AND table_name = 'dbsc_metadata'";
            return conn.Query<long>(sql).First() > 0;
        }

        protected override bool CheckoutAndUpdateIsSupported(out string whyNot)
        {
            whyNot = null;
            return true;
        }

        protected override bool ImportIsSupported(out string whyNot)
        {
            whyNot = null;
            return true;
        }

        protected override void ImportData(SqlUpdateOptions options, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadataAlreadyEscaped)
        {
            PgImportOperation import = new PgImportOperation(options, tablesToImportAlreadyEscaped, allTablesExceptMetadataAlreadyEscaped);
            import.Run();
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
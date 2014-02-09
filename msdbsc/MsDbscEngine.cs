using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Dapper;
using dbsc.Core;
using System.Diagnostics;
using System.Data;

namespace dbsc.SqlServer
{
    class MsDbscEngine : SqlDbscEngine<SqlCheckoutOptions, SqlUpdateOptions, MsDbscDbConnection>
    {
        protected override DbConnectionInfo GetSystemDatabaseConnectionInfo(DbConnectionInfo targetDatabase)
        {
            DbConnectionInfo systemInfo = targetDatabase.Clone();
            systemInfo.Database = "master";
            return systemInfo;
        }

        protected override MsDbscDbConnection OpenConnection(DbConnectionInfo connectionInfo)
        {
            return new MsDbscDbConnection(connectionInfo);
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

        protected override bool MetadataTableExists(MsDbscDbConnection conn)
        {
            string sql = @"SELECT count(*) FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME = 'dbsc_metadata'";
            return conn.Query<int>(sql).First() > 0;
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

        private class Table
        {
            public string TableSchema { get; set; }
            public string TableName { get; set; }
        }

        protected override ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(MsDbscDbConnection conn)
        {
            string sql = @"SELECT TABLE_SCHEMA AS TableSchema, TABLE_NAME AS TableName FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME <> 'dbsc_metadata'";
            List<string> tables = conn.Query<Table>(sql).Select(t => MsDbscDbConnection.QuoteSqlServerIdentifier(t.TableSchema, t.TableName)).ToList();
            return tables;
        }

        protected override void ImportData(SqlUpdateOptions options, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadataAlreadyEscaped)
        {
            MsImportOperation import = new MsImportOperation(options, tablesToImportAlreadyEscaped, allTablesExceptMetadataAlreadyEscaped);
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
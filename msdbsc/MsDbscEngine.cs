using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using Dapper;
using dbsc.Core;
using dbsc.Core.Sql;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.SqlServer
{
    class MsDbscEngine : SqlDbscEngine<SqlServerConnectionSettings, SqlServerCheckoutSettings, SqlServerImportSettings, SqlServerUpdateSettings, MsDbscDbConnection, TableAndRule<SqlServerTable, TableWithSchemaSpecificationWithCustomSelect>>
    {
        protected override char QueryParamChar { get { return '@'; } }
        
        protected override SqlServerConnectionSettings GetSystemDatabaseConnectionInfo(SqlServerConnectionSettings targetDatabase)
        {
            SqlServerConnectionSettings systemInfo = targetDatabase.Clone();
            systemInfo.Database = "master";
            return systemInfo;
        }

        protected override MsDbscDbConnection OpenConnection(SqlServerConnectionSettings connectionInfo)
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

        public override ICollection<TableAndRule<SqlServerTable, TableWithSchemaSpecificationWithCustomSelect>> GetTablesToImport(SqlServerUpdateSettings updateSettings)
        {
            using (MsDbscDbConnection conn = OpenConnection(updateSettings.TargetDatabase))
            {
                SqlServerImportTableCalculator tableCalculator = new SqlServerImportTableCalculator();
                return tableCalculator.GetTablesToImport(conn, updateSettings.ImportOptions.TablesToImportSpecifications);
            }
        }

        public override void ImportData(SqlServerUpdateSettings updateSettings, ICollection<TableAndRule<SqlServerTable, TableWithSchemaSpecificationWithCustomSelect>> tablesToImport)
        {
            ICollection<SqlServerTable> tablesExceptMetadata;
            using (MsDbscDbConnection conn = OpenConnection(updateSettings.TargetDatabase))
            {
                tablesExceptMetadata = conn.GetTablesExceptMetadata();
            }

            MsImportOperation import = new MsImportOperation(updateSettings, tablesToImport, tablesExceptMetadata);
            import.Run();
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
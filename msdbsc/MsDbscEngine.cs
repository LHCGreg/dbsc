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

        private class Table
        {
            public string TableSchema { get; set; }
            public string TableName { get; set; }
        }

        private class DefaultSchema
        {
            public string DefaultSchemaName { get; set; }
        }

        private ICollection<SqlServerTable> GetTablesExceptMetadata(MsDbscDbConnection conn)
        {
            string sql = @"SELECT TABLE_SCHEMA AS TableSchema, TABLE_NAME AS TableName FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME <> 'dbsc_metadata'";
            List<Table> tables = conn.Query<Table>(sql).ToList();
            return tables.Select(table => new SqlServerTable(table.TableSchema, table.TableName)).ToList();
        }

        public override ICollection<TableAndRule<SqlServerTable, TableWithSchemaSpecificationWithCustomSelect>> GetTablesToImport(SqlServerUpdateSettings updateSettings)
        {
            using (MsDbscDbConnection conn = OpenConnection(updateSettings.TargetDatabase))
            {
                ICollection<SqlServerTable> eligibleTables = GetTablesExceptMetadata(conn);

                if(updateSettings.ImportOptions.TablesToImportSpecifications == null)
                {
                    List<TableAndRule<SqlServerTable, TableWithSchemaSpecificationWithCustomSelect>> tables = new List<TableAndRule<SqlServerTable, TableWithSchemaSpecificationWithCustomSelect>>();
                    foreach (SqlServerTable table in eligibleTables)
                    {
                        tables.Add(new TableAndRule<SqlServerTable,TableWithSchemaSpecificationWithCustomSelect>(table,
                            TableWithSchemaSpecificationWithCustomSelect.Star));
                    }
                    return tables;
                }
                else
                {
                    // Get default schema
                    string defaultSchemaSql = "SELECT SCHEMA_NAME() AS DefaultSchemaName";
                    DefaultSchema defaultSchema = conn.Query<DefaultSchema>(defaultSchemaSql).FirstOrDefault();
                    if(defaultSchema == null)
                    {
                        throw new DbscException("Error: No default schema returned from SELECT SCHEMA_NAME()");
                    }

                    // Set default schema to use when calculating
                    updateSettings.ImportOptions.TablesToImportSpecifications.DefaultSchema = defaultSchema.DefaultSchemaName;
                    ICollection<TableAndRule<SqlServerTable, TableWithSchemaSpecificationWithCustomSelect>> tablesToImport = updateSettings.ImportOptions.TablesToImportSpecifications.GetTablesToImport(eligibleTables);
                    
                    // Throw an error if the user specifified an import table specification file and there were any lines
                    // that specified a table directly (no wildcards) and the table does not exist. The user probably made a mistake.
                    ICollection<TableWithSchemaSpecificationWithCustomSelect> nonMatchingNonWildcardSpecs = updateSettings.ImportOptions.TablesToImportSpecifications.GetNonWildcardTableSpecsThatDontExist(eligibleTables);
                    if (nonMatchingNonWildcardSpecs.Count > 0)
                    {
                        throw new DbscException(string.Format("The following tables were specified to be imported but do not exist: {0}",
                            string.Join(", ", nonMatchingNonWildcardSpecs.Select(spec => new SqlServerTable(spec.Schema != null ? spec.Schema.Pattern[0].String : defaultSchema.DefaultSchemaName, spec.Table.Pattern[0].String)))));
                    }

                    return tablesToImport;
                }
            }
        }

        public override void ImportData(SqlServerUpdateSettings updateSettings, ICollection<TableAndRule<SqlServerTable, TableWithSchemaSpecificationWithCustomSelect>> tablesToImport)
        {
            ICollection<SqlServerTable> tablesExceptMetadata;
            using (MsDbscDbConnection conn = OpenConnection(updateSettings.TargetDatabase))
            {
                tablesExceptMetadata = GetTablesExceptMetadata(conn);
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
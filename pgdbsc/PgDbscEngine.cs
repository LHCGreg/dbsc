using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.ImportTableSpecification;
using dbsc.Core.Sql;

namespace dbsc.Postgres
{
    class PgDbscEngine : SqlDbscEngine<DbConnectionInfo, PgCheckoutSettings, PgImportSettings, PgUpdateSettings, PgDbscDbConnection, TableAndRule<PgTable, TableWithSchemaSpecificationWithCustomSelect>>
    {
        public PgDbscEngine()
        {
            ;
        }

        protected override char QueryParamChar { get { return '@'; } }

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

        private ICollection<PgTable> GetTablesExceptMetadata(PgDbscDbConnection conn)
        {
            string sql = @"SELECT table_schema, table_name FROM information_schema.tables
WHERE table_schema NOT LIKE 'pg_%' AND table_schema <> 'information_schema'
AND table_type = 'BASE TABLE'
AND table_name <> 'dbsc_metadata'";

            List<PgTable> tables = conn.Query<Table>(sql).Select(t => new PgTable(t.table_schema, t.table_name)).ToList();
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

        public override ICollection<TableAndRule<PgTable, TableWithSchemaSpecificationWithCustomSelect>> GetTablesToImport(PgUpdateSettings updateSettings)
        {
            using (PgDbscDbConnection conn = OpenConnection(updateSettings.TargetDatabase))
            {
                ICollection<PgTable> eligibleTables = GetTablesExceptMetadata(conn);

                if (updateSettings.ImportOptions.TablesToImportSpecifications == null)
                {
                    List<TableAndRule<PgTable, TableWithSchemaSpecificationWithCustomSelect>> tables = new List<TableAndRule<PgTable, TableWithSchemaSpecificationWithCustomSelect>>();
                    foreach (PgTable table in eligibleTables)
                    {
                        tables.Add(new TableAndRule<PgTable, TableWithSchemaSpecificationWithCustomSelect>(table,
                            TableWithSchemaSpecificationWithCustomSelect.Star));
                    }
                    return tables;
                }
                else
                {
                    const string defaultSchema = "public";
                    updateSettings.ImportOptions.TablesToImportSpecifications.DefaultSchema = defaultSchema;
                    ICollection<TableAndRule<PgTable, TableWithSchemaSpecificationWithCustomSelect>> tablesToImport = updateSettings.ImportOptions.TablesToImportSpecifications.GetTablesToImport(eligibleTables);

                    // Throw an error if the user specifified an import table specification file and there were any lines
                    // that specified a table directly (no wildcards) and the table does not exist. The user probably made a mistake.
                    ICollection<TableWithSchemaSpecificationWithCustomSelect> nonMatchingNonWildcardSpecs = updateSettings.ImportOptions.TablesToImportSpecifications.GetNonWildcardTableSpecsThatDontExist(eligibleTables);
                    if (nonMatchingNonWildcardSpecs.Count > 0)
                    {
                        throw new DbscException(string.Format("The following tables were specified to be imported but do not exist: {0}",
                            string.Join(", ", nonMatchingNonWildcardSpecs.Select(spec => new PgTable(spec.Schema != null ? spec.Schema.Pattern[0].String : defaultSchema, spec.Table.Pattern[0].String)))));
                    }

                    return tablesToImport;
                }
            }
        }

        public override void ImportData(PgUpdateSettings updateSettings, ICollection<TableAndRule<PgTable, TableWithSchemaSpecificationWithCustomSelect>> tablesToImport)
        {
            ICollection<PgTable> tablesExceptMetadata;
            using (PgDbscDbConnection conn = OpenConnection(updateSettings.TargetDatabase))
            {
                tablesExceptMetadata = GetTablesExceptMetadata(conn);
            }

            PgImportOperation import = new PgImportOperation(updateSettings, tablesToImport, tablesExceptMetadata);
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
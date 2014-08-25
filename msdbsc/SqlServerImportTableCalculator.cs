using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.ImportTableSpecification;
using dbsc.Core.Sql;

namespace dbsc.SqlServer
{
    class SqlServerImportTableCalculator : ImportTableCalculator<SqlServerTable, TableWithSchemaSpecificationWithCustomSelect, TableWithSchemaSpecificationWithCustomSelectCollection<SqlServerTable>, MsDbscDbConnection>
    {
        protected override TableWithSchemaSpecificationWithCustomSelect GetStarSpec()
        {
            return TableWithSchemaSpecificationWithCustomSelect.Star;
        }

        private class DefaultSchema
        {
            public string DefaultSchemaName { get; set; }
        }

        protected override void OnCalculatingUsingSpecs(MsDbscDbConnection conn, TableWithSchemaSpecificationWithCustomSelectCollection<SqlServerTable> tableSpecs)
        {
            // Get default schema
            string defaultSchemaSql = "SELECT SCHEMA_NAME() AS DefaultSchemaName";
            DefaultSchema defaultSchema = conn.Query<DefaultSchema>(defaultSchemaSql).FirstOrDefault();
            if (defaultSchema == null)
            {
                throw new DbscException("Error: No default schema returned from SELECT SCHEMA_NAME()");
            }

            // Set default schema to use when calculating
            tableSpecs.DefaultSchema = defaultSchema.DefaultSchemaName;
        }

        protected override ICollection<SqlServerTable> GetTablesExceptMetadata(MsDbscDbConnection conn)
        {
            return conn.GetTablesExceptMetadata();
        }

        protected override SqlServerTable GetTableFromNonWildcardSpec(TableWithSchemaSpecificationWithCustomSelect spec, TableWithSchemaSpecificationWithCustomSelectCollection<SqlServerTable> tableSpecs)
        {
            string schema;
            if (spec.Schema != null)
            {
                schema = spec.Schema.Pattern[0].String;
            }
            else
            {
                schema = tableSpecs.DefaultSchema;
            }

            string table = spec.Table.Pattern[0].String;

            return new SqlServerTable(schema, table);
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
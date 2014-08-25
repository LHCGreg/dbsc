using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.ImportTableSpecification;
using dbsc.Core.Sql;

namespace dbsc.Postgres
{
    class PgImportTableCalculator : ImportTableCalculator<PgTable, TableWithSchemaSpecificationWithCustomSelect, TableWithSchemaSpecificationWithCustomSelectCollection<PgTable>, PgDbscDbConnection>
    {
        const string DefaultSchema = "public";
        
        protected override TableWithSchemaSpecificationWithCustomSelect GetStarSpec()
        {
            return TableWithSchemaSpecificationWithCustomSelect.Star;
        }

        protected override void OnCalculatingUsingSpecs(PgDbscDbConnection conn, TableWithSchemaSpecificationWithCustomSelectCollection<PgTable> tableSpecs)
        {
            tableSpecs.DefaultSchema = DefaultSchema;
        }

        protected override ICollection<PgTable> GetTablesExceptMetadata(PgDbscDbConnection conn)
        {
            return conn.GetTablesExceptMetadata();
        }

        protected override PgTable GetTableFromNonWildcardSpec(TableWithSchemaSpecificationWithCustomSelect spec, TableWithSchemaSpecificationWithCustomSelectCollection<PgTable> tableSpecs)
        {
            string schema;
            if (spec.Schema != null)
            {
                schema = spec.Schema.Pattern[0].String;
            }
            else
            {
                schema = DefaultSchema;
            }

            string table = spec.Table.Pattern[0].String;

            return new PgTable(schema, table);
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
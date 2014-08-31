using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.Core
{
    public abstract class ImportTableCalculator<TTable, TTableSpec, TTableSpecCollection, TConnection>
        where TTableSpecCollection : TableSpecificationCollection<TTableSpec, TTable>
        where TTableSpec : ITableSpecification
    {
        public ImportTableCalculator()
        {
            ;
        }

        public ICollection<TableAndRule<TTable, TTableSpec>> GetTablesToImport(TConnection conn, TTableSpecCollection tableSpecs)
        {
            ICollection<TTable> eligibleTables = GetTablesExceptMetadata(conn);

            if (tableSpecs == null)
            {
                List<TableAndRule<TTable, TTableSpec>> tables = new List<TableAndRule<TTable, TTableSpec>>();
                foreach (TTable table in eligibleTables)
                {
                    tables.Add(new TableAndRule<TTable, TTableSpec>(table, GetStarSpec()));
                }
                return tables;
            }
            else
            {
                // Get default schema
                OnCalculatingUsingSpecs(conn, tableSpecs);
                ICollection<TableAndRule<TTable, TTableSpec>> tablesToImport = tableSpecs.GetTablesToImport(eligibleTables);

                // Throw an error if the user specified an import table specification file and there were any lines
                // that specified a table directly (no wildcards) and the table does not exist. The user probably made a mistake.
                ICollection<TTableSpec> nonMatchingNonWildcardSpecs = tableSpecs.GetNonWildcardTableSpecsThatDontExist(eligibleTables);
                if (nonMatchingNonWildcardSpecs.Count > 0)
                {
                    throw new DbscException(string.Format("The following tables were specified to be imported but do not exist: {0}",
                        string.Join(", ", nonMatchingNonWildcardSpecs.Select(spec => GetTableFromNonWildcardSpec(spec, tableSpecs)))));
                }

                return tablesToImport;
            }
        }

        protected abstract ICollection<TTable> GetTablesExceptMetadata(TConnection conn);
        protected abstract TTableSpec GetStarSpec();
        protected abstract TTable GetTableFromNonWildcardSpec(TTableSpec spec, TTableSpecCollection tableSpecs);

        protected virtual void OnCalculatingUsingSpecs(TConnection conn, TTableSpecCollection tableSpecs)
        {
            ;
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
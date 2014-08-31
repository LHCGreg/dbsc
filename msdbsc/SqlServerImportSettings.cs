using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.SqlServer
{
    class SqlServerImportSettings : IImportSettings<SqlServerConnectionSettings>, ICloneable
    {
        public SqlServerConnectionSettings SourceDatabase { get; set; }
        public TableWithSchemaSpecificationWithCustomSelectCollection<SqlServerTable> TablesToImportSpecifications { get; set; }

        public SqlServerImportSettings(SqlServerConnectionSettings sourceDatabase, TableWithSchemaSpecificationWithCustomSelectCollection<SqlServerTable> tablesToImportSpecifications)
        {
            SourceDatabase = sourceDatabase;
            TablesToImportSpecifications = tablesToImportSpecifications;
        }

        public SqlServerImportSettings Clone()
        {
            SqlServerConnectionSettings clonedSourceDatabase = SourceDatabase.Clone();

            TableWithSchemaSpecificationWithCustomSelectCollection<SqlServerTable> clonedSpecifications = null;
            if (TablesToImportSpecifications != null)
            {
                clonedSpecifications = TablesToImportSpecifications.Clone();
            }
            SqlServerImportSettings clone = new SqlServerImportSettings(clonedSourceDatabase, clonedSpecifications);
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
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
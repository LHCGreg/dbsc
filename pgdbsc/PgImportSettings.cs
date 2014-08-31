using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.Postgres
{
    class PgImportSettings : IImportSettings<DbConnectionInfo>, ICloneable
    {
        public DbConnectionInfo SourceDatabase { get; set; }
        public TableWithSchemaSpecificationWithCustomSelectCollection<PgTable> TablesToImportSpecifications { get; set; }

        public PgImportSettings(DbConnectionInfo sourceDatabase, TableWithSchemaSpecificationWithCustomSelectCollection<PgTable> tablesToImportSpecifications)
        {
            SourceDatabase = sourceDatabase;
            TablesToImportSpecifications = tablesToImportSpecifications;
        }

        public PgImportSettings Clone()
        {
            DbConnectionInfo clonedSourceDatabase = SourceDatabase.Clone();

            TableWithSchemaSpecificationWithCustomSelectCollection<PgTable> clonedSpecifications = null;
            if (TablesToImportSpecifications != null)
            {
                clonedSpecifications = TablesToImportSpecifications.Clone();
            }
            PgImportSettings clone = new PgImportSettings(clonedSourceDatabase, clonedSpecifications);
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
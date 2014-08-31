using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.Mongo
{
    class MongoImportSettings : IImportSettings<DbConnectionInfo>, ICloneable
    {
        public DbConnectionInfo SourceDatabase { get; set; }
        public TableWithoutSchemaSpecificationCollection<MongoTable> CollectionsToImportSpecifications { get; set; }

        public MongoImportSettings(DbConnectionInfo sourceDatabase, TableWithoutSchemaSpecificationCollection<MongoTable> collectionsToImportSpecifications)
        {
            SourceDatabase = sourceDatabase;
            CollectionsToImportSpecifications = collectionsToImportSpecifications;
        }

        public MongoImportSettings Clone()
        {
            DbConnectionInfo clonedSourceDatabase = SourceDatabase.Clone();

            TableWithoutSchemaSpecificationCollection<MongoTable> clonedSpecifications = null;
            if (CollectionsToImportSpecifications != null)
            {
                clonedSpecifications = CollectionsToImportSpecifications.Clone();
            }
            MongoImportSettings clone = new MongoImportSettings(clonedSourceDatabase, clonedSpecifications);
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
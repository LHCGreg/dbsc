using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    public class TableWithSchemaSpecificationCollection<TTable> : TableSpecificationCollection<TableWithSchemaSpecification, TTable>
        where TTable : ITableWithSchema
    {
        public string DefaultSchema { get; set; }
        
        public TableWithSchemaSpecificationCollection(IList<TableWithSchemaSpecification> tableSpecifications, string defaultSchema)
            : base(tableSpecifications)
        {
            DefaultSchema = defaultSchema;
        }

        protected override TableWithSchemaSpecification GetStarSpecification()
        {
            return TableWithSchemaSpecification.Star;
        }

        protected override MatchResult MatchTableWithSpec(TableWithSchemaSpecification spec, TTable table)
        {
            return spec.Match(table, DefaultSchema);
        }

        public TableWithSchemaSpecificationCollection<TTable> Clone()
        {
            return new TableWithSchemaSpecificationCollection<TTable>(new List<TableWithSchemaSpecification>(TableSpecifications), DefaultSchema);
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
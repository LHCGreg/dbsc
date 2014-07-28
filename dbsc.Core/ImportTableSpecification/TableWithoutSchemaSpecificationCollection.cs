using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    public class TableWithoutSchemaSpecificationCollection<TTable> : TableSpecificationCollection<TableWithoutSchemaSpecification, TTable>
        where TTable : ITable
    {
        public TableWithoutSchemaSpecificationCollection(IList<TableWithoutSchemaSpecification> tableSpecifications)
            : base(tableSpecifications)
        {

        }

        protected override TableWithoutSchemaSpecification GetStarSpecification()
        {
            return TableWithoutSchemaSpecification.Star;
        }

        protected override MatchResult MatchTableWithSpec(TableWithoutSchemaSpecification spec, TTable table)
        {
            return spec.Match(table);
        }

        public TableWithoutSchemaSpecificationCollection<TTable> Clone()
        {
            return new TableWithoutSchemaSpecificationCollection<TTable>(new List<TableWithoutSchemaSpecification>(TableSpecifications));
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
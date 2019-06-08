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

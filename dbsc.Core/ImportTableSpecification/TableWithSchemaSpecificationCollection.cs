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

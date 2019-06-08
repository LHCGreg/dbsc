using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    public class TableWithSchemaSpecificationWithCustomSelectCollection<TTable> : TableSpecificationCollection<TableWithSchemaSpecificationWithCustomSelect, TTable>
        where TTable : ITableWithSchema
    {
        public string DefaultSchema { get; set; }

        public TableWithSchemaSpecificationWithCustomSelectCollection(IList<TableWithSchemaSpecificationWithCustomSelect> tableSpecifications, string defaultSchema)
            : base(tableSpecifications)
        {
            DefaultSchema = defaultSchema;
        }

        protected override TableWithSchemaSpecificationWithCustomSelect GetStarSpecification()
        {
            return TableWithSchemaSpecificationWithCustomSelect.Star;
        }

        protected override MatchResult MatchTableWithSpec(TableWithSchemaSpecificationWithCustomSelect spec, TTable table)
        {
            return spec.Match(table, DefaultSchema);
        }

        public TableWithSchemaSpecificationWithCustomSelectCollection<TTable> Clone()
        {
            return new TableWithSchemaSpecificationWithCustomSelectCollection<TTable>(new List<TableWithSchemaSpecificationWithCustomSelect>(TableSpecifications), DefaultSchema);
        }
    }
}

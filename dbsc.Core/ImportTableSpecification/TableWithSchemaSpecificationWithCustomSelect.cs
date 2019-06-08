using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    public class TableWithSchemaSpecificationWithCustomSelect : TableWithSchemaSpecification
    {
        /// <summary>
        /// Null to use default for matched tables
        /// </summary>
        public string CustomSelect { get; private set; }
        
        public TableWithSchemaSpecificationWithCustomSelect(TableSpecificationFragment schema, TableSpecificationFragment table, bool negated, bool defaultSchemaIsCaseSensitive, string customSelect)
            : base(schema, table, negated, defaultSchemaIsCaseSensitive)
        {
            CustomSelect = customSelect;
        }

        public TableWithSchemaSpecificationWithCustomSelect(TableSpecificationFragment schema, TableSpecificationFragment table, bool negated, bool defaultSchemaIsCaseSensitive)
            : base(schema, table, negated, defaultSchemaIsCaseSensitive)
        {
            CustomSelect = null;
        }

        public static new TableWithSchemaSpecificationWithCustomSelect Star
        {
            get
            {
                return new TableWithSchemaSpecificationWithCustomSelect(schema: TableSpecificationFragment.Star, table: TableSpecificationFragment.Star, negated: false, defaultSchemaIsCaseSensitive: false);
            }
        }
    }
}

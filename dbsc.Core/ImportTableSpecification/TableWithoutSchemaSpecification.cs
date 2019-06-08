using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    public class TableWithoutSchemaSpecification : ITableSpecification
    {
        public bool Negated { get; private set; }
        public TableSpecificationFragment Table { get; private set; }

        public TableWithoutSchemaSpecification(TableSpecificationFragment table, bool negated)
        {
            if (table == null) throw new ArgumentNullException("table");
            Table = table;
            Negated = negated;
        }

        public bool HasWildcards
        {
            get
            {
                return Table.HasWildcards;
            }
        }

        public MatchResult Match(ITable table)
        {
            bool matches = Table.Matches(table.Table);
            if (matches && !Negated)
            {
                return MatchResult.PositiveMatch;
            }
            else if (matches && Negated)
            {
                return MatchResult.NegativeMatch;
            }
            else
            {
                return MatchResult.NoMatch;
            }
        }

        public static TableWithoutSchemaSpecification Star
        {
            get
            {
                return new TableWithoutSchemaSpecification(table: TableSpecificationFragment.Star, negated: false);
            }
        }

        public override string ToString()
        {
            if (!Negated)
            {
                return Table.ToString();
            }
            else
            {
                return "-" + Table.ToString();
            }
        }
    }
}

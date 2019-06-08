using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    public class TableWithSchemaSpecification : ITableSpecification
    {
        public bool Negated { get; private set; }

        /// <summary>
        /// Null if schema not specified
        /// </summary>
        public TableSpecificationFragment Schema { get; private set; }

        public TableSpecificationFragment Table { get; private set; }

        public bool DefaultSchemaIsCaseSensitive { get; private set; }

        public TableWithSchemaSpecification(TableSpecificationFragment schema, TableSpecificationFragment table, bool negated, bool defaultSchemaIsCaseSensitive)
        {
            if (table == null) throw new ArgumentNullException("table");
            Schema = schema;
            Table = table;
            Negated = negated;
            DefaultSchemaIsCaseSensitive = defaultSchemaIsCaseSensitive;
        }

        public MatchResult Match(ITableWithSchema table, string defaultSchema)
        {
            bool matchSoFar = true;

            // Match schema
            if (Schema != null)
            {
                matchSoFar = Schema.Matches(table.Schema);
            }
            else
            {
                TableSpecificationFragment defaultSchemaFrag = new TableSpecificationFragment(defaultSchema, DefaultSchemaIsCaseSensitive);
                matchSoFar = defaultSchemaFrag.Matches(table.Schema);
            }

            // Match table
            if (matchSoFar)
            {
                matchSoFar = Table.Matches(table.Table);
            }

            if (!matchSoFar)
            {
                return MatchResult.NoMatch;
            }
            else if(Negated)
            {
                return MatchResult.NegativeMatch;
            }
            else
            {
                return MatchResult.PositiveMatch;
            }
        }

        public static TableWithSchemaSpecification Star
        {
            get
            {
                // DefaultSchemaIsCaseSensitive does not matter here because the schema is specified.
                // This static property can be used regardless of the case-sensitivity of the database.
                return new TableWithSchemaSpecification(schema: TableSpecificationFragment.Star, table: TableSpecificationFragment.Star, negated: false, defaultSchemaIsCaseSensitive: false);
            }
        }

        public bool HasWildcards
        {
            get
            {
                return (Schema != null && Schema.HasWildcards) || Table.HasWildcards;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (Negated)
            {
                builder.Append("-");
            }
            if (Schema != null)
            {
                builder.Append(Schema.ToString());
                builder.Append(".");
            }
            builder.Append(Table.ToString());
            return builder.ToString();
        }
    }
}

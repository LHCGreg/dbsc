using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    /// <summary>
    /// An ordered collection of table specification rules.
    /// </summary>
    /// <typeparam name="TTableSpecification"></typeparam>
    /// <typeparam name="TTable"></typeparam>
    public abstract class TableSpecificationCollection<TTableSpecification, TTable>
        where TTableSpecification : ITableSpecification
    {
        public IList<TTableSpecification> TableSpecifications { get; private set; }

        public TableSpecificationCollection(IList<TTableSpecification> tableSpecifications)
        {
            TableSpecifications = tableSpecifications;
        }

        protected abstract TTableSpecification GetStarSpecification();
        protected abstract MatchResult MatchTableWithSpec(TTableSpecification spec, TTable table);

        public ICollection<TableAndRule<TTable, TTableSpecification>> GetTablesToImport(ICollection<TTable> eligibleTables)
        {
            IList<TTableSpecification> effectiveSpecs = TableSpecifications;

            // If all negative, there's an implicit "everything" rule at the top.
            if (TableSpecifications.Count > 0 && TableSpecifications.All(spec => spec.Negated))
            {
                List<TTableSpecification> effectiveSpecsList = new List<TTableSpecification>(TableSpecifications.Count + 1);
                effectiveSpecsList.Add(GetStarSpecification());
                effectiveSpecsList.AddRange(TableSpecifications);
                effectiveSpecs = effectiveSpecsList;
            }

            // Last match wins
            List<TableAndRule<TTable, TTableSpecification>> tablesToImport = new List<TableAndRule<TTable, TTableSpecification>>();
            foreach (TTable table in eligibleTables)
            {
                MatchResult currentResult = MatchResult.NoMatch;
                TTableSpecification currentResultRule = default(TTableSpecification);
                foreach (TTableSpecification spec in effectiveSpecs)
                {
                    MatchResult specResult = MatchTableWithSpec(spec, table);
                    if(specResult != MatchResult.NoMatch)
                    {
                        currentResult = specResult;
                        currentResultRule = spec;
                    }
                }

                if (currentResult == MatchResult.PositiveMatch)
                {
                    tablesToImport.Add(new TableAndRule<TTable, TTableSpecification>(table, currentResultRule));
                }
            }

            return tablesToImport;
        }

        /// <summary>
        /// Returns the non-negative rules that do not contain wildcards (ie just plain table names) that do not match any
        /// tables in <paramref name="eligibleTables"/>. The user probably made a mistake in the import table list file.
        /// </summary>
        /// <param name="eligibleTables"></param>
        /// <returns></returns>
        public ICollection<TTableSpecification> GetNonWildcardTableSpecsThatDontExist(ICollection<TTable> eligibleTables)
        {
            List<TTableSpecification> tablesNotFound = new List<TTableSpecification>();

            List<TTableSpecification> nonWildcardTableSpecs = TableSpecifications.Where(spec => !spec.Negated && !spec.HasWildcards).ToList();

            // O(n^2) :-\ Ehhh, you shouldn't be specifying a large number of tables anyway, wildcards and exclusions should keeps import lists short
            foreach (TTableSpecification nonWildcardTableSpec in nonWildcardTableSpecs)
            {
                bool found = eligibleTables.Any(table => MatchTableWithSpec(nonWildcardTableSpec, table) == MatchResult.PositiveMatch);
                if (!found)
                {
                    tablesNotFound.Add(nonWildcardTableSpec);
                }
            }

            return tablesNotFound;
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, TableSpecifications.Select(spec => spec.ToString()));
        }
    }
}

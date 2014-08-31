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
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.Core.Tests.ImportTableSpecification
{
    /// <summary>
    /// Comparisons used during testing that should not necessarily be on the real class.
    /// </summary>
    public static class Comparisons
    {
        public static int TableAndRuleComparision(TableAndRule<TableWithSchema, TableWithSchemaSpecification> x, TableAndRule<TableWithSchema, TableWithSchemaSpecification> y)
        {
            int result = string.Compare(x.Table.Schema, y.Table.Schema, StringComparison.Ordinal);
            if (result != 0)
            {
                return result;
            }

            result = string.Compare(x.Table.Table, y.Table.Table, StringComparison.Ordinal);
            if (result != 0)
            {
                return result;
            }

            return TableWithSchemaSpecificationComparision(x.Rule, y.Rule);
        }

        public static int TableWithSchemaSpecificationComparision(TableWithSchemaSpecification x, TableWithSchemaSpecification y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null && y != null)
            {
                return -1;
            }
            else if (x != null && y == null)
            {
                return 1;
            }

            int result = x.Negated.CompareTo(y.Negated);
            if (result != 0) return result;

            result = TableSpecificationFragmentComparision(x.Schema, y.Schema);
            if (result != 0) return result;

            result = TableSpecificationFragmentComparision(x.Table, y.Table);
            if (result != 0) return result;

            // Only compare DefaultSchemaIsCaseSensitive if schema is null, this lets us compare with *.*
            if (x.Schema == null)
            {
                result = x.DefaultSchemaIsCaseSensitive.CompareTo(y.DefaultSchemaIsCaseSensitive);
                if (result != 0) return result;
            }

            return 0;
        }

        public static int TableWithSchemaSpecificationWithCustomSelectComparison(TableWithSchemaSpecificationWithCustomSelect x, TableWithSchemaSpecificationWithCustomSelect y)
        {
            int result = TableWithSchemaSpecificationComparision(x, y);
            if (result != 0)
            {
                return result;
            }

            return string.Compare(x.CustomSelect, y.CustomSelect, StringComparison.Ordinal);
        }

        public static int TableSpecificationFragmentComparision(TableSpecificationFragment x, TableSpecificationFragment y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            else if (x == null && y != null)
            {
                return -1;
            }
            else if (x != null && y == null)
            {
                return 1;
            }

            if (x.Pattern.Count != y.Pattern.Count)
            {
                return x.Pattern.Count.CompareTo(y.Pattern.Count);
            }

            for (int i = 0; i < x.Pattern.Count; i++)
            {
                StringOrWildcard xchunk = x.Pattern[i];
                StringOrWildcard ychunk = y.Pattern[i];
                if (!xchunk.Equals(ychunk))
                {
                    return -1; // Only equality/inequality matters, we don't care about ordering
                }
            }

            return 0;
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
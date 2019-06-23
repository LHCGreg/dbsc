using dbsc.Core.ImportTableSpecification;
using System;
using System.Collections.Generic;
using System.Text;

namespace dbsc.Core.Tests.ImportTableSpecification
{
    public class TableAndRuleEqualityComparer : IEqualityComparer<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>
    {
        public bool Equals(TableAndRule<TableWithSchema, TableWithSchemaSpecification> x, TableAndRule<TableWithSchema, TableWithSchemaSpecification> y)
        {
            return Comparisons.TableAndRuleComparision(x, y) == 0;
        }

        public int GetHashCode(TableAndRule<TableWithSchema, TableWithSchemaSpecification> obj)
        {
            return Comparisons.TableAndRuleHashcode(obj);
        }
    }
}

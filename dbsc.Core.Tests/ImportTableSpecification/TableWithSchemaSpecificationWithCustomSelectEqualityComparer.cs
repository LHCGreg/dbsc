using dbsc.Core.ImportTableSpecification;
using System;
using System.Collections.Generic;
using System.Text;

namespace dbsc.Core.Tests.ImportTableSpecification
{
    public class TableWithSchemaSpecificationWithCustomSelectEqualityComparer : IEqualityComparer<TableWithSchemaSpecificationWithCustomSelect>
    {
        public bool Equals(TableWithSchemaSpecificationWithCustomSelect x, TableWithSchemaSpecificationWithCustomSelect y)
        {
            return Comparisons.TableWithSchemaSpecificationWithCustomSelectComparison(x, y) == 0;
        }

        public int GetHashCode(TableWithSchemaSpecificationWithCustomSelect obj)
        {
            return Comparisons.TableWithSchemaSpecificationWithCustomSelectHashCode(obj);
        }
    }
}

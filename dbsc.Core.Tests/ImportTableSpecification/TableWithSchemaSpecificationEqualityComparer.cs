using dbsc.Core.ImportTableSpecification;
using System;
using System.Collections.Generic;
using System.Text;

namespace dbsc.Core.Tests.ImportTableSpecification
{
    public class TableWithSchemaSpecificationEqualityComparer : IEqualityComparer<TableWithSchemaSpecification>
    {
        public bool Equals(TableWithSchemaSpecification x, TableWithSchemaSpecification y)
        {
            return Comparisons.TableWithSchemaSpecificationComparison(x, y) == 0;
        }

        public int GetHashCode(TableWithSchemaSpecification obj)
        {
            return Comparisons.TableWithSchemaSpecificationHashcode(obj);
        }
    }
}

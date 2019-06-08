using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    public interface ITableWithSchemaSpecification : ITableSpecification
    {
        TableSpecificationFragment Schema { get; }
        TableSpecificationFragment Table { get; }
        bool DefaultSchemaIsCaseSensitive { get; }
    }
}

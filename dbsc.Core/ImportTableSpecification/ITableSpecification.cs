using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    public interface ITableSpecification
    {
        bool Negated { get; }
        bool HasWildcards { get; }
    }
}

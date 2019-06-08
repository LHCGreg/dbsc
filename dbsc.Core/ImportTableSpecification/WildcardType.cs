using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    // Use byte to minimize space used by CharOrWildcard struct
    public enum WildcardType : byte
    {
        None = 0,
        Star
    }
}

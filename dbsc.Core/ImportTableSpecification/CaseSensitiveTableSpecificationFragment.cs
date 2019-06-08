using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    public class CaseSensitiveTableSpecificationFragment : TableSpecificationFragment
    {
        public CaseSensitiveTableSpecificationFragment(IList<StringOrWildcard> pattern)
            : base(pattern, caseSensitive: true)
        {

        }
    }
}

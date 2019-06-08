using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    public class CaseInsensitiveTableSpecificationFragment : TableSpecificationFragment
    {
        public CaseInsensitiveTableSpecificationFragment(IList<StringOrWildcard> pattern)
            : base(pattern, caseSensitive: false)
        {

        }
    }
}

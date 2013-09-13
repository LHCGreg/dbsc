using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface ISqlCheckoutOptions<TUpdateOptions> : ICheckoutOptions<TUpdateOptions>
        where TUpdateOptions : ISqlUpdateOptions
    {
        string CreationTemplate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface ITableWithSchema 
    {
        string Schema { get; }
        string Table { get; }
    }
}

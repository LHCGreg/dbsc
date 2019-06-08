using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.ImportTableSpecification
{
    /// <summary>
    /// Just a tuple with named properties
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <typeparam name="TTableSpecification"></typeparam>
    public struct TableAndRule<TTable, TTableSpecification>
    {
        public TTable Table { get; private set; }
        public TTableSpecification Rule { get; private set; }

        public TableAndRule(TTable table, TTableSpecification rule)
            : this()
        {
            Table = table;
            Rule = rule;
        }
    }
}

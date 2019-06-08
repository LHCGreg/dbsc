using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public class TableWithSchema : ITableWithSchema
    {
        public string Schema { get; private set; }
        public string Table { get; private set; }
        
        public TableWithSchema(string schema, string table)
        {
            Schema = schema;
            Table = table;
        }
    }
}

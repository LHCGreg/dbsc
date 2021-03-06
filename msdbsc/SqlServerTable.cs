﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;

namespace dbsc.SqlServer
{
    class SqlServerTable : ITableWithSchema
    {
        public string Schema { get; private set; }
        public string Table { get; private set; }

        public SqlServerTable(string schema, string table)
        {
            Schema = schema;
            Table = table;
        }

        /// <summary>
        /// The table in two-part [schema].[table] format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return MsDbscDbConnection.QuoteSqlServerIdentifier(Schema, Table);
        }
    }
}

/*
 Copyright 2014 Greg Najda

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
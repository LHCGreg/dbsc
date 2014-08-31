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
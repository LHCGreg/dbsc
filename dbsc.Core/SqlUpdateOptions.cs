using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public class SqlUpdateOptions : ISqlUpdateOptions
    {
        public string Directory { get; set; }
        public DbConnectionInfo TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public ImportOptions ImportOptions { get; set; }

        public SqlUpdateOptions(DbConnectionInfo targetDatabase)
        {
            Directory = Environment.CurrentDirectory;
            TargetDatabase = targetDatabase;
        }

        public SqlUpdateOptions(SqlCheckoutOptions checkoutOptions)
        {
            Directory = checkoutOptions.Directory;
            TargetDatabase = checkoutOptions.TargetDatabase.Clone();
            Revision = checkoutOptions.Revision;

            if (checkoutOptions.ImportOptions != null)
            {
                ImportOptions = checkoutOptions.ImportOptions.Clone();
            }
        }

        public SqlUpdateOptions Clone()
        {
            SqlUpdateOptions clone = new SqlUpdateOptions(TargetDatabase.Clone());
            clone.Directory = Directory;
            clone.Revision = Revision;

            if (ImportOptions != null)
            {
                clone.ImportOptions = ImportOptions.Clone();
            }

            return clone;
        }

        IUpdateOptions IUpdateOptions.Clone()
        {
            return Clone();
        }
    }
}

/*
 Copyright 2013 Greg Najda

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public class SqlCheckoutOptions : ISqlCheckoutOptions<SqlUpdateOptions>
    {
        public string Directory { get; set; }
        public DbConnectionInfo TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public string CreationTemplate { get; set; }
        public ImportOptions ImportOptions { get; set; }

        public SqlCheckoutOptions(DbConnectionInfo targetDatabase)
        {
            TargetDatabase = targetDatabase;
            Directory = Environment.CurrentDirectory;
            CreationTemplate = "CREATE DATABASE $DatabaseName$";
        }

        public SqlUpdateOptions UpdateOptions
        {
            get
            {
                return new SqlUpdateOptions(this);
            }
        }

        public SqlCheckoutOptions Clone()
        {
            SqlCheckoutOptions clone = new SqlCheckoutOptions(TargetDatabase.Clone());
            clone.Directory = Directory;
            clone.Revision = Revision;
            clone.CreationTemplate = CreationTemplate;

            if (ImportOptions != null)
            {
                clone.ImportOptions = ImportOptions.Clone();
            }

            return clone;
        }

        ICheckoutOptions<SqlUpdateOptions> ICheckoutOptions<SqlUpdateOptions>.Clone()
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
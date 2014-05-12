using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.SqlServer
{
    class SqlServerUpdateSettings : ISqlUpdateOptions<SqlServerConnectionSettings, ImportOptions<SqlServerConnectionSettings>>
    {
        public string Directory { get; set; }
        public SqlServerConnectionSettings TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public ImportOptions<SqlServerConnectionSettings> ImportOptions { get; set; }

        public SqlServerUpdateSettings(SqlServerConnectionSettings targetDatabase)
        {
            Directory = Environment.CurrentDirectory;
            TargetDatabase = targetDatabase;
        }

        public SqlServerUpdateSettings(SqlServerCheckoutSettings checkoutSettings)
        {
            Directory = checkoutSettings.Directory;
            TargetDatabase = checkoutSettings.TargetDatabase.Clone();
            Revision = checkoutSettings.Revision;

            if (checkoutSettings.ImportOptions != null)
            {
                ImportOptions = checkoutSettings.ImportOptions.Clone();
            }
        }

        public SqlServerUpdateSettings Clone()
        {
            SqlServerUpdateSettings clone = new SqlServerUpdateSettings(TargetDatabase.Clone());
            clone.Directory = Directory;
            clone.Revision = Revision;

            if (ImportOptions != null)
            {
                clone.ImportOptions = ImportOptions.Clone();
            }

            return clone;
        }

        IUpdateOptions<SqlServerConnectionSettings, ImportOptions<SqlServerConnectionSettings>>  IUpdateOptions<SqlServerConnectionSettings, ImportOptions<SqlServerConnectionSettings>>.Clone()
        {
            return Clone();
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
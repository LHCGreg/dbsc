using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.SqlServer
{
    class SqlServerCheckoutSettings : ISqlCheckoutSettings<SqlServerConnectionSettings, SqlServerImportSettings, SqlServerUpdateSettings>
    {
        public string Directory { get; set; }
        public SqlServerConnectionSettings TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public string CreationTemplate { get; set; }
        public SqlServerImportSettings ImportOptions { get; set; }

        public SqlServerCheckoutSettings(SqlServerConnectionSettings targetDatabase)
        {
            TargetDatabase = targetDatabase;
            Directory = Environment.CurrentDirectory;
            CreationTemplate = dbsc.Core.Options.DBCreateTemplateOptionBundle.DefaultSQLTemplate;
        }

        public SqlServerUpdateSettings UpdateOptions { get { return new SqlServerUpdateSettings(this); } }

        public SqlServerCheckoutSettings Clone()
        {
            SqlServerCheckoutSettings clone = new SqlServerCheckoutSettings(TargetDatabase.Clone());
            clone.Directory = Directory;
            clone.Revision = Revision;
            clone.CreationTemplate = CreationTemplate;

            if (ImportOptions != null)
            {
                clone.ImportOptions = ImportOptions.Clone();
            }

            return clone;
        }


        ICheckoutOptions<SqlServerConnectionSettings, SqlServerImportSettings, SqlServerUpdateSettings> ICheckoutOptions<SqlServerConnectionSettings, SqlServerImportSettings, SqlServerUpdateSettings>.Clone()
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
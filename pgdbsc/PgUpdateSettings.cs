using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.Postgres
{
    class PgUpdateSettings : ISqlUpdateSettings<DbConnectionInfo, PgImportSettings>
    {
        public string Directory { get; set; }
        public DbConnectionInfo TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public PgImportSettings ImportOptions { get; set; }

        public PgUpdateSettings(DbConnectionInfo targetDatabase)
        {
            Directory = Environment.CurrentDirectory;
            TargetDatabase = targetDatabase;
        }

        public PgUpdateSettings(PgCheckoutSettings checkoutSettings)
        {
            Directory = checkoutSettings.Directory;
            TargetDatabase = checkoutSettings.TargetDatabase.Clone();
            Revision = checkoutSettings.Revision;

            if (checkoutSettings.ImportOptions != null)
            {
                ImportOptions = checkoutSettings.ImportOptions.Clone();
            }
        }

        public PgUpdateSettings Clone()
        {
            PgUpdateSettings clone = new PgUpdateSettings(TargetDatabase.Clone());
            clone.Directory = Directory;
            clone.Revision = Revision;

            if (ImportOptions != null)
            {
                clone.ImportOptions = ImportOptions.Clone();
            }

            return clone;
        }

        IUpdateSettings<DbConnectionInfo, PgImportSettings> IUpdateSettings<DbConnectionInfo, PgImportSettings>.Clone()
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
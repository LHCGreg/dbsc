using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;

namespace dbsc.Mongo
{
    class MongoUpdateSettings : IUpdateSettings<DbConnectionInfo, MongoImportSettings>
    {
        public string Directory { get; set; }
        public DbConnectionInfo TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public MongoImportSettings ImportOptions { get; set; }

        public MongoUpdateSettings(DbConnectionInfo targetDatabase)
        {
            Directory = Environment.CurrentDirectory;
            TargetDatabase = targetDatabase;
            Revision = null;
            ImportOptions = null;
        }

        public MongoUpdateSettings(MongoCheckoutOptions checkoutSettings)
        {
            Directory = checkoutSettings.Directory;
            TargetDatabase = checkoutSettings.TargetDatabase.Clone();
            Revision = checkoutSettings.Revision;

            if (checkoutSettings.ImportOptions != null)
            {
                ImportOptions = checkoutSettings.ImportOptions.Clone();
            }
        }

        public MongoUpdateSettings Clone()
        {
            MongoUpdateSettings clone = new MongoUpdateSettings(this.TargetDatabase.Clone());
            clone.Directory = this.Directory;
            clone.Revision = this.Revision;

            if (this.ImportOptions != null)
            {
                clone.ImportOptions = this.ImportOptions.Clone();
            }
            return clone;
        }

        IUpdateSettings<DbConnectionInfo, MongoImportSettings> IUpdateSettings<DbConnectionInfo, MongoImportSettings>.Clone()
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
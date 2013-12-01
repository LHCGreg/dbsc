using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;

namespace dbsc.Mongo
{
    class MongoUpdateOptions : IUpdateOptions
    {
        public string Directory { get; set; }
        public DbConnectionInfo TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public ImportOptions ImportOptions { get; set; }

        public MongoUpdateOptions(DbConnectionInfo targetDatabase)
        {
            Directory = Environment.CurrentDirectory;
            TargetDatabase = targetDatabase;
            Revision = null;
            ImportOptions = null;
        }

        public MongoUpdateOptions(MongoCheckoutOptions checkoutOptions)
        {
            Directory = checkoutOptions.Directory;
            TargetDatabase = checkoutOptions.TargetDatabase.Clone();
            Revision = checkoutOptions.Revision;

            if (checkoutOptions.ImportOptions != null)
            {
                ImportOptions = checkoutOptions.ImportOptions.Clone();
            }
        }

        public MongoUpdateOptions Clone()
        {
            MongoUpdateOptions clone = new MongoUpdateOptions(this.TargetDatabase.Clone());
            clone.Directory = this.Directory;
            clone.Revision = this.Revision;

            if (this.ImportOptions != null)
            {
                clone.ImportOptions = this.ImportOptions.Clone();
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
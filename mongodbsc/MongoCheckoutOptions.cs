using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;

namespace dbsc.Mongo
{
    class MongoCheckoutOptions : ICheckoutOptions<DbConnectionInfo, ImportOptions<DbConnectionInfo>, MongoUpdateOptions>
    {
        public string Directory { get; set; }
        public DbConnectionInfo TargetDatabase { get; set; }
        public int? Revision { get; set; }

        /// <summary>
        /// Null if no creation template.
        /// </summary>
        public string CreationTemplate { get; set; }

        public ImportOptions<DbConnectionInfo> ImportOptions { get; set; }

        public MongoUpdateOptions UpdateOptions { get { return new MongoUpdateOptions(this); } }

        public MongoCheckoutOptions(DbConnectionInfo targetDatabase)
        {
            Directory = Environment.CurrentDirectory;
            TargetDatabase = targetDatabase;
            Revision = null;
            CreationTemplate = null;
            ImportOptions = null;
        }

        public MongoCheckoutOptions Clone()
        {
            MongoCheckoutOptions clone = new MongoCheckoutOptions(this.TargetDatabase.Clone());
            clone.Directory = this.Directory;
            clone.Revision = this.Revision;
            clone.CreationTemplate = this.CreationTemplate;

            if (this.ImportOptions != null)
            {
                clone.ImportOptions = this.ImportOptions.Clone();
            }

            return clone;
        }

        ICheckoutOptions<DbConnectionInfo, ImportOptions<DbConnectionInfo>, MongoUpdateOptions> ICheckoutOptions<DbConnectionInfo, ImportOptions<DbConnectionInfo>, MongoUpdateOptions>.Clone()
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
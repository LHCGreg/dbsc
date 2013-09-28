using dbsc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Mongo
{
    class MongoCommandLineArgs : BaseCommandLineArgs
    {
        public MongoCommandLineArgs()
        {
            ;
        }

        public MongoCheckoutOptions GetCheckoutOptions()
        {
            DbConnectionInfo targetDbInfo = new DbConnectionInfo(server: TargetDbServer, database: TargetDb, port: TargetDbPort, username: Username, password: Password);
            MongoCheckoutOptions options = new MongoCheckoutOptions(targetDbInfo);
            options.Directory = ScriptDirectory;
            options.Revision = Revision;
            options.ImportOptions = GetImportOptions();
            return options;
        }

        public MongoUpdateOptions GetUpdateOptions()
        {
            DbConnectionInfo targetDbInfo = new DbConnectionInfo(server: TargetDbServer, database: TargetDb, port: TargetDbPort, username: Username, password: Password);
            MongoUpdateOptions options = new MongoUpdateOptions(targetDbInfo);
            options.Directory = ScriptDirectory;
            options.Revision = Revision;
            options.ImportOptions = GetImportOptions();
            return options;
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
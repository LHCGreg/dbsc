using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using dbsc.Core;

namespace dbsc.SqlServer
{
    class CommandLineArgs : BaseCommandLineArgs
    {
        private string m_targetDbServer = @"localhost";
        public string TargetDbServer { get { return m_targetDbServer; } set { m_targetDbServer = value; } }

        public CommandLineArgs()
        {
            ;
        }

        public override OptionSet GetOptionSet()
        {
            OptionSet options = base.GetOptionSet();
            options.Add("targetDbServer=", @"Server of the target database. Defaults to localhost.", arg => TargetDbServer = arg);
            return options;
        }

        public CheckoutOptions GetCheckoutOptions()
        {
            DbConnectionInfo targetDbInfo = new DbConnectionInfo(server: TargetDbServer, database: TargetDb, username: Username, password: Password);
            CheckoutOptions options = new CheckoutOptions(targetDbInfo);
            options.CreationTemplate = GetDbCreationTemplate();
            options.Directory = ScriptDirectory;
            options.Revision = Revision;
            options.ImportOptions = GetImportOptions();
            return options;
        }

        public UpdateOptions GetUpdateOptions()
        {
            DbConnectionInfo targetDbInfo = new DbConnectionInfo(server: TargetDbServer, database: TargetDb, username: Username, password: Password);
            UpdateOptions options = new UpdateOptions(targetDbInfo);
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
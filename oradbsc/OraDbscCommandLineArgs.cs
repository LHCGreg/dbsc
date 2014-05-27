using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.Sql;
using dbsc.Core.Options;

namespace dbsc.Oracle
{
    class OraDbscCommandLineArgs : BaseCommandLineArgs, ICommandLineArgs<SqlCheckoutSettings, SqlUpdateSettings>
    {
        private TargetDBOptionBundle _targetDB = new TargetDBOptionBundle()
        {
            AuthenticationRequired = false,
            TargetDBMessage = "Oracle service name of the target database. Defaults to the master database name detected from script names. Unlike other dbsc flavors, the database must already exist."
        };
        private TargetDBPortOptionBundle _targetDBPort = new TargetDBPortOptionBundle();
        
        // Importing with Oracle not supported at this time

        public OraDbscCommandLineArgs()
        {
            ExtraOptions.Add(_targetDB);
            ExtraOptions.Add(_targetDBPort);
        }

        private DbConnectionInfo GetTargetConnectionSettings()
        {
            return new DbConnectionInfo(
                server: _targetDB.TargetDBServer,
                database: _targetDB.TargetDB,
                port: _targetDBPort.TargetDBPort,
                username: _targetDB.Username,
                password: _targetDB.Password
            );
        }

        public SqlCheckoutSettings GetCheckoutSettings()
        {
            DbConnectionInfo connectionSettings = GetTargetConnectionSettings();

            SqlCheckoutSettings checkoutSettings = new SqlCheckoutSettings(connectionSettings);
            checkoutSettings.CreationTemplate = null;
            checkoutSettings.Directory = this.ScriptDirectory;
            checkoutSettings.ImportOptions = null; // Importing not supported at this time
            checkoutSettings.Revision = this.Revison;
            return checkoutSettings;
        }

        public SqlUpdateSettings GetUpdateSettings()
        {
            DbConnectionInfo connectionSettings = GetTargetConnectionSettings();

            SqlUpdateSettings updateSettings = new SqlUpdateSettings(connectionSettings);
            updateSettings.Directory = this.ScriptDirectory;
            updateSettings.ImportOptions = null; // Importing not supported at this time
            updateSettings.Revision = this.Revison;
            return updateSettings;
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
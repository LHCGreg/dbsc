using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.Options;
using dbsc.SqlServer.ImportTableSpecification;

namespace dbsc.SqlServer
{
    class MsDbscCommandLineArgs : BaseCommandLineArgs, ICommandLineArgs<SqlServerCheckoutSettings, SqlServerUpdateSettings>
    {
        private TargetDBOptionBundle _targetDB = new TargetDBOptionBundle()
        {
            AuthenticationRequired = false,
            UsernameMessage = "Username to use to log in to the target database. If not specified, log in with integrated security.",
            PasswordMessage = "Password to use to log in to the target database. If not specified and username is not specified, log in with integrated security. If not specified and username is specified, you will be prompted for your password."
        };

        private DBCreateTemplateOptionBundle _template = new DBCreateTemplateOptionBundle(DBCreateTemplateOptionBundle.DefaultSQLTemplate);
        
        private SourceDBOptionBundle _sourceDB = new SourceDBOptionBundle()
        {
            AuthenticationRequired = false,
            UsernameMessage = "Username to use to log in to the source database. If not specified, log in with integrated security.",
            PasswordMessage = "Password to use to log in to the source database. If not specified and username is not specified, log in with integrated security. If not specified and username is specified, you will be prompted for your password."
        };

        private SqlServerImportTableListOptionBundle _importSpecificationOptionBundle = new SqlServerImportTableListOptionBundle();

        public MsDbscCommandLineArgs()
        {
            ExtraOptions.Add(_targetDB);
            ExtraOptions.Add(_template);
            ExtraOptions.Add(_sourceDB);
            ExtraOptions.Add(_importSpecificationOptionBundle);
        }

        private SqlServerConnectionSettings GetTargetConnectionSettings()
        {
            return new SqlServerConnectionSettings(
                server: _targetDB.TargetDBServer,
                database: _targetDB.TargetDB,
                username: _targetDB.Username,
                password: _targetDB.Password
            );
        }

        private SqlServerImportSettings GetImportSettings()
        {
            if (_sourceDB.SourceDBServer != null)
            {
                SqlServerConnectionSettings sourceConnectionSettings = new SqlServerConnectionSettings(
                    server: _sourceDB.SourceDBServer,
                    database: _sourceDB.SourceDB,
                    username: _sourceDB.SourceUsername,
                    password: _sourceDB.SourcePassword
                );

                SqlServerImportSettings importSettings = new SqlServerImportSettings(sourceConnectionSettings, _importSpecificationOptionBundle.ImportTableSpecifications);

                return importSettings;
            }
            else
            {
                return null;
            }
        }

        public SqlServerCheckoutSettings GetCheckoutSettings()
        {
            SqlServerConnectionSettings connectionSettings = GetTargetConnectionSettings();

            SqlServerCheckoutSettings checkoutSettings = new SqlServerCheckoutSettings(connectionSettings);
            checkoutSettings.CreationTemplate = _template.Template;
            checkoutSettings.ImportOptions = GetImportSettings();
            checkoutSettings.Directory = this.ScriptDirectory;
            checkoutSettings.Revision = this.Revison;

            return checkoutSettings;
        }

        public SqlServerUpdateSettings GetUpdateSettings()
        {
            SqlServerConnectionSettings connectionSettings = GetTargetConnectionSettings();
            SqlServerUpdateSettings updateSettings = new SqlServerUpdateSettings(connectionSettings);
            updateSettings.Directory = this.ScriptDirectory;
            updateSettings.Revision = this.Revison;
            updateSettings.ImportOptions = GetImportSettings();

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
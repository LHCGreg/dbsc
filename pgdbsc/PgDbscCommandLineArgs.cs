using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.Options;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.Postgres
{
    class PgDbscCommandLineArgs : BaseCommandLineArgs, ICommandLineArgs<SqlCheckoutOptions, SqlUpdateOptions>
    {
        private TargetDBOptionBundle _targetDB = new TargetDBOptionBundle() { IntegratedSecuritySupported = Utils.RunningOnWindows() };
        private TargetDBPortOptionBundle _targetDBPort = new TargetDBPortOptionBundle();
        private DBCreateTemplateOptionBundle _template = new DBCreateTemplateOptionBundle(DBCreateTemplateOptionBundle.DefaultSQLTemplate);
        private SourceDBOptionBundle _sourceDB = new SourceDBOptionBundle() { IntegratedSecuritySupported = Utils.RunningOnWindows() };
        private SourceDBPortOptionBundle _sourceDBPort = new SourceDBPortOptionBundle();
        private ImportTableListFileOptionBundle _importTableListFile = new ImportTableListFileOptionBundle();
        
        public PgDbscCommandLineArgs()
        {
            ExtraOptions.Add(_targetDB);
            ExtraOptions.Add(_targetDBPort);
            ExtraOptions.Add(_template);
            ExtraOptions.Add(_sourceDB);
            ExtraOptions.Add(_sourceDBPort);
            ExtraOptions.Add(_importTableListFile);
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

        private ImportOptions<DbConnectionInfo> GetImportSettings()
        {
            if (_sourceDB.SourceDBServer != null)
            {
                DbConnectionInfo sourceConnectionSettings = new DbConnectionInfo(
                    server: _sourceDB.SourceDBServer,
                    database: _sourceDB.SourceDB,
                    port: _sourceDBPort.SourceDBPort,
                    username: _sourceDB.SourceUsername,
                    password: _sourceDB.SourcePassword
                );

                ImportOptions<DbConnectionInfo> importSettings = new ImportOptions<DbConnectionInfo>(sourceConnectionSettings);
                importSettings.TablesToImport = _importTableListFile.TablesToImport;
                return importSettings;
            }
            else
            {
                return null;
            }
        }

        public SqlCheckoutOptions GetCheckoutSettings()
        {
            DbConnectionInfo connectionSettings = GetTargetConnectionSettings();

            SqlCheckoutOptions checkoutSettings = new SqlCheckoutOptions(connectionSettings);
            checkoutSettings.CreationTemplate = _template.Template;
            checkoutSettings.ImportOptions = GetImportSettings();
            checkoutSettings.Directory = this.ScriptDirectory;
            checkoutSettings.Revision = this.Revison;

            return checkoutSettings;
        }

        public SqlUpdateOptions GetUpdateSettings()
        {
            DbConnectionInfo connectionSettings = GetTargetConnectionSettings();
            SqlUpdateOptions updateSettings = new SqlUpdateOptions(connectionSettings);
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
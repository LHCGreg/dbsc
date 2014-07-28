using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.Options;
using dbsc.Core.Sql;

namespace dbsc.MySql
{
    class MyDbscCommandLineArgs : BaseCommandLineArgs, ICommandLineArgs<SqlCheckoutSettings, SqlUpdateSettings>
    {
        private TargetDBOptionBundle _targetDB = new TargetDBOptionBundle();
        private TargetDBPortOptionBundle _targetDBPort = new TargetDBPortOptionBundle();
        private DBCreateTemplateOptionBundle _template = new DBCreateTemplateOptionBundle(DBCreateTemplateOptionBundle.DefaultSQLTemplate);
        private SourceDBOptionBundle _sourceDB = new SourceDBOptionBundle();
        private SourceDBPortOptionBundle _sourceDBPort = new SourceDBPortOptionBundle();
        private ImportTableListFileOptionBundle _importTableListFile = new ImportTableListFileOptionBundle();

        public MyDbscCommandLineArgs()
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

        private ImportSettingsWithTableList<DbConnectionInfo> GetImportSettings()
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

                ImportSettingsWithTableList<DbConnectionInfo> importSettings = new ImportSettingsWithTableList<DbConnectionInfo>(sourceConnectionSettings);
                importSettings.TablesToImport = _importTableListFile.TablesToImport;
                return importSettings;
            }
            else
            {
                return null;
            }
        }

        public SqlCheckoutSettings GetCheckoutSettings()
        {
            DbConnectionInfo connectionSettings = GetTargetConnectionSettings();

            SqlCheckoutSettings checkoutSettings = new SqlCheckoutSettings(connectionSettings);
            checkoutSettings.CreationTemplate = _template.Template;
            checkoutSettings.ImportOptions = GetImportSettings();
            checkoutSettings.Directory = this.ScriptDirectory;
            checkoutSettings.Revision = this.Revison;

            return checkoutSettings;
        }

        public SqlUpdateSettings GetUpdateSettings()
        {
            DbConnectionInfo connectionSettings = GetTargetConnectionSettings();
            SqlUpdateSettings updateSettings = new SqlUpdateSettings(connectionSettings);
            updateSettings.Directory = this.ScriptDirectory;
            updateSettings.Revision = this.Revison;
            updateSettings.ImportOptions = GetImportSettings();

            return updateSettings;
        }
    }
}

// Copyright (C) 2014 Greg Najda
//
// This file is part of mydbsc.
//
// mydbsc is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// mydbsc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with mydbsc.  If not, see <http://www.gnu.org/licenses/>.
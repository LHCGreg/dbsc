using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.ImportTableSpecification;
using dbsc.Core.Options;
using dbsc.Core.Sql;

namespace dbsc.MySql
{
    class MyDbscCommandLineArgs : BaseCommandLineArgs, ICommandLineArgs<MySqlCheckoutSettings, MySqlUpdateSettings>
    {
        private TargetDBOptionBundle _targetDB = new TargetDBOptionBundle();
        private TargetDBPortOptionBundle _targetDBPort = new TargetDBPortOptionBundle();
        private DBCreateTemplateOptionBundle _template = new DBCreateTemplateOptionBundle(DBCreateTemplateOptionBundle.DefaultSQLTemplate);
        private SourceDBOptionBundle _sourceDB = new SourceDBOptionBundle();
        private SourceDBPortOptionBundle _sourceDBPort = new SourceDBPortOptionBundle();
        private ImportTableListOptionBundle<TableWithoutSchemaSpecificationCollection<MySqlTable>> _importSpecificationOptionBundle =
            new ImportTableListOptionBundle<TableWithoutSchemaSpecificationCollection<MySqlTable>>(
                new MySqlImportTableListParser(), ImportTableListOptionBundle<TableWithoutSchemaSpecificationCollection<MySqlTable>>.WildcardsAndNegationsDescription);


        public MyDbscCommandLineArgs()
        {
            ExtraOptions.Add(_targetDB);
            ExtraOptions.Add(_targetDBPort);
            ExtraOptions.Add(_template);
            ExtraOptions.Add(_sourceDB);
            ExtraOptions.Add(_sourceDBPort);
            ExtraOptions.Add(_importSpecificationOptionBundle);
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

        private MySqlImportSettings GetImportSettings()
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

                MySqlImportSettings importSettings = new MySqlImportSettings(sourceConnectionSettings, _importSpecificationOptionBundle.ImportTableSpecifications);
                return importSettings;
            }
            else
            {
                return null;
            }
        }

        public MySqlCheckoutSettings GetCheckoutSettings()
        {
            DbConnectionInfo connectionSettings = GetTargetConnectionSettings();

            MySqlCheckoutSettings checkoutSettings = new MySqlCheckoutSettings(connectionSettings);
            checkoutSettings.CreationTemplate = _template.Template;
            checkoutSettings.ImportOptions = GetImportSettings();
            checkoutSettings.Directory = this.ScriptDirectory;
            checkoutSettings.Revision = this.Revison;

            return checkoutSettings;
        }

        public MySqlUpdateSettings GetUpdateSettings()
        {
            DbConnectionInfo connectionSettings = GetTargetConnectionSettings();
            MySqlUpdateSettings updateSettings = new MySqlUpdateSettings(connectionSettings);
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
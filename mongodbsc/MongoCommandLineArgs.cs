using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.ImportTableSpecification;
using dbsc.Core.Options;

namespace dbsc.Mongo
{
    class MongoCommandLineArgs : BaseCommandLineArgs, ICommandLineArgs<MongoCheckoutOptions, MongoUpdateSettings>
    {
        private TargetDBOptionBundle _targetDB = new TargetDBOptionBundle()
        {
            AuthenticationRequired = false,
            UsernameMessage = "Username to use to log in to the target database. Only specify if the target MongoDB has authentication enabled.",
            PasswordMessage = "Password to use to log in to the target database. If username is specified and no password is specified, you will be prompted for your password. Only specify if the target MongoDB has authentication enabled."
        };

        private TargetDBPortOptionBundle _targetDBPort = new TargetDBPortOptionBundle();

        private DBCreateTemplateOptionBundle _template = new DBCreateTemplateOptionBundle(defaultTemplate: null)
        {
            HelpMessage = "File with a template javascript file to run after creating the database in a checkout. $DatabaseName$ will be replaced with the database name."
        };

        private SourceDBOptionBundle _sourceDB = new SourceDBOptionBundle()
        {
            AuthenticationRequired = false,
            UsernameMessage = "Username to use to log in to the source database. Only specify if the source MongoDB has authentication enabled.",
            PasswordMessage = "Password to use to log in to the source database. If username is specified and no password is specified, you will be prompted for your password. Only specify if the source MongoDB has authentication enabled."
        };

        private SourceDBPortOptionBundle _sourceDBPort = new SourceDBPortOptionBundle();

        private ImportTableListOptionBundle<TableWithoutSchemaSpecificationCollection<MongoTable>> _importSpecificationOptionBundle =
            new ImportTableListOptionBundle<TableWithoutSchemaSpecificationCollection<MongoTable>>(
                new MongoImportTableListParser(), ImportTableListOptionBundle<TableWithoutSchemaSpecificationCollection<MongoTable>>.MongoWildcardsAndNegationsDescription);

        
        public MongoCommandLineArgs()
        {
            base.BasicOptions.ScriptDirectoryDescription = "Directory with js scripts to run. If not specified, defaults to the current directory.";
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

        private MongoImportSettings GetImportSettings()
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

                MongoImportSettings importSettings = new MongoImportSettings(sourceConnectionSettings, _importSpecificationOptionBundle.ImportTableSpecifications);
                return importSettings;
            }
            else
            {
                return null;
            }
        }

        public MongoCheckoutOptions GetCheckoutSettings()
        {
            DbConnectionInfo connectionSettings = GetTargetConnectionSettings();

            MongoCheckoutOptions checkoutOptions = new MongoCheckoutOptions(connectionSettings);
            checkoutOptions.CreationTemplate = _template.Template;
            checkoutOptions.Directory = this.ScriptDirectory;
            checkoutOptions.ImportOptions = GetImportSettings();
            checkoutOptions.Revision = this.Revison;

            return checkoutOptions;
        }

        public MongoUpdateSettings GetUpdateSettings()
        {
            DbConnectionInfo connectionSettings = GetTargetConnectionSettings();
            MongoUpdateSettings updateSettings = new MongoUpdateSettings(connectionSettings);
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
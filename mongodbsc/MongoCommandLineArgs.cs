using dbsc.Core;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace dbsc.Mongo
{
    class MongoCommandLineArgs : BaseCommandLineArgs
    {
        public string DbTemplateFilePath { get; private set; }
        
        public MongoCommandLineArgs()
        {
            ;
        }

        public string GetDbCreationTemplate()
        {
            if (DbTemplateFilePath == null)
            {
                return null;
            }
            else
            {
                try
                {
                    return File.ReadAllText(DbTemplateFilePath);
                }
                catch (Exception ex)
                {
                    throw new OptionException(string.Format("Error reading DB creation template: {0}.", ex.Message), "dbCreationTemplate");
                }
            }
        }

        public override OptionSet GetOptionSet()
        {
            OptionSet options = base.GetOptionSet();

            options.Add("dbCreateTemplate=",
                "Javascript file to run when creating a database in a checkout. $DatabaseName$ will be replaced with the database name. mongodbsc does not actually create the database because in MongoDB, a database is created when you first do something with it. This is a good place to grant permissions. The script will run in the admin database.",
                arg => DbTemplateFilePath = arg);
            return options;
        }

        public MongoCheckoutOptions GetCheckoutOptions()
        {
            DbConnectionInfo targetDbInfo = new DbConnectionInfo(server: TargetDbServer, database: TargetDb, port: TargetDbPort, username: Username, password: Password);
            MongoCheckoutOptions options = new MongoCheckoutOptions(targetDbInfo);
            options.Directory = ScriptDirectory;
            options.Revision = Revision;
            options.ImportOptions = GetImportOptions();
            options.CreationTemplate = GetDbCreationTemplate();
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
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public class SqlCommandLineArgs : BaseCommandLineArgs
    {
        public string DbTemplateFilePath { get; private set; }

        public string GetDbCreationTemplate()
        {
            if (DbTemplateFilePath == null)
            {
                return "CREATE DATABASE $DatabaseName$";
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
                "File with a template to use when creating the database in a checkout. $DatabaseName$ will be replaced with the database name. If not specified, a simple \"CREATE DATABASE $DatabaseName$\" will be used. This is a good place to set database options or grant permissions.",
                arg => DbTemplateFilePath = arg );
            return options;
        }

        public virtual SqlCheckoutOptions GetCheckoutOptions()
        {
            DbConnectionInfo targetDbInfo = new DbConnectionInfo(server: TargetDbServer, database: TargetDb, port: TargetDbPort, username: Username, password: Password);
            SqlCheckoutOptions options = new SqlCheckoutOptions(targetDbInfo);
            options.CreationTemplate = GetDbCreationTemplate();
            options.Directory = ScriptDirectory;
            options.Revision = Revision;
            options.ImportOptions = GetImportOptions();
            return options;
        }

        public virtual SqlUpdateOptions GetUpdateOptions()
        {
            DbConnectionInfo targetDbInfo = new DbConnectionInfo(server: TargetDbServer, database: TargetDb, port: TargetDbPort, username: Username, password: Password);
            SqlUpdateOptions options = new SqlUpdateOptions(targetDbInfo);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using dbsc.Core;
using dbsc.Core.Options;

namespace dbsc.Postgres
{
    class PgSourceDBOptionBundle : IOptionBundle
    {
        public string SourceDBServer { get; private set; }
        public string SourceDB { get; private set; }
        public string SourceUsername { get; private set; }

        private string _sourcePassword = null;
        public string SourcePassword
        {
            get
            {
                if (!UseIntegratedSecurity)
                {
                    return _sourcePassword;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _sourcePassword = value;
            }
        }

        private bool _useIntegratedSecurity = false;
        public bool UseIntegratedSecurity { get { return _useIntegratedSecurity; } set { _useIntegratedSecurity = value; } }
        
        public void AddToOptionSet(OptionSet optionSet)
        {
            optionSet.Add("sourceDbServer=", SourceDBOptionBundle.GetDefaultSourceDbServerOptionText(), arg => SourceDBServer = arg);
            optionSet.Add("sourceDb=", SourceDBOptionBundle.DefaultSourceDbOptionText, arg => SourceDB = arg);
            optionSet.Add("sourceUsername=", "Username to use to log in to the source database.", arg => SourceUsername = arg);
            optionSet.Add("sourcePassword=", "Password to use to log in to the source database. If not specified, you will be prompted for your password unless the -sourceSSPI flag is specified.", arg => SourcePassword = arg);
            optionSet.Add("sourceSSPI|sourceIntegratedSecurity", "Use SSPI (Windows login) authentication with the source database.", argExistence => UseIntegratedSecurity = (argExistence != null));
        }

        public void Validate()
        {
            if (SourceDB != null && SourceDBServer == null)
            {
                throw new DbscOptionException("sourceDbServer must be specified if importing data.");
            }

            if (SourceUsername != null && SourceDBServer == null)
            {
                throw new DbscOptionException("sourceDbServer must be specified if importing data.");
            }

            if (SourcePassword != null && SourceDBServer == null)
            {
                throw new DbscOptionException("sourceDbServer must be specified if importing data.");
            }

            if (SourceDBServer != null && SourceUsername == null)
            {
                throw new DbscOptionException("sourceUsername must be specified if importing data.");
            }

            if (UseIntegratedSecurity && !Utils.RunningOnWindows())
            {
                throw new DbscOptionException("SSPI authentication is only supported on Windows.");
            }
        }

        public void PostValidate()
        {
            if (SourceUsername != null && SourcePassword == null && !UseIntegratedSecurity)
            {
                Console.Write("Password for {0} on source database server {1}: ", SourceUsername, SourceDBServer);
                SourcePassword = Utils.ReadPassword();
            }
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
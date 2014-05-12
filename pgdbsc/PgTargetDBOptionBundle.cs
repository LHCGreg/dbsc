using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using dbsc.Core;
using dbsc.Core.Options;

namespace dbsc.Postgres
{
    class PgTargetDBOptionBundle : IOptionBundle
    {
        public string TargetDB { get; private set; }

        private string m_targetDBServer = "localhost";
        public string TargetDBServer { get { return m_targetDBServer; } set { m_targetDBServer = value; } }

        public string Username { get; private set; }

        private string _password = null;
        public string Password
        {
            get
            {
                if (!UseIntegratedSecurity)
                {
                    return _password;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _password = value;
            }
        }

        private bool _useIntegratedSecurity = false;
        public bool UseIntegratedSecurity { get { return _useIntegratedSecurity; } set { _useIntegratedSecurity = value; } }

        public void AddToOptionSet(OptionSet optionSet)
        {
            optionSet.Add("targetDb=", TargetDBOptionBundle.DefaultTargetDbOptionText, arg => TargetDB = arg);
            optionSet.Add("targetDbServer=", TargetDBOptionBundle.DefaultTargetDbServerOptionText, arg => TargetDBServer = arg);
            optionSet.Add("u|username=", "Username to use to log in to the target database. REQUIRED.", arg => Username = arg);
            optionSet.Add("p|password=", "Password to use to log in to the target database. If not specified, you will be prompted for your password unless the -SSPI flag is specified.", arg => Password = arg);
            optionSet.Add("SSPI|integratedSecurity", "Use SSPI (Windows login) authentication.", argExistence => UseIntegratedSecurity = (argExistence != null));
        }

        public void Validate()
        {
            if (Username == null)
            {
                throw new DbscOptionException("Username must be specified with -u username.");
            }

            if (UseIntegratedSecurity && !Utils.RunningOnWindows())
            {
                throw new DbscOptionException("SSPI authentication is only supported on Windows.");
            }
        }

        public void PostValidate()
        {
            if (Password == null && !UseIntegratedSecurity)
            {
                Console.Write("Password for {0} on target database {1}: ", Username, TargetDBServer);
                Password = Utils.ReadPassword();
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;

namespace dbsc.Core.Options
{
    public class TargetDBOptionBundle : IOptionBundle
    {
        public string TargetDB { get; private set; }

        private string m_targetDBServer = "localhost";
        public string TargetDBServer { get { return m_targetDBServer; } set { m_targetDBServer = value; } }

        public string Username { get; private set; }
        public string Password { get; private set; }

        private bool _authenticationRequired = true;
        public bool AuthenticationRequired { get { return _authenticationRequired; } set { _authenticationRequired = value; } }

        private const string DefaultUsernameMessage = "Username to use to log in to the target database. REQUIRED.";
        private const string DefaultPasswordMessage = "Password to use to log in to the target database. If not specified, you will be prompted for your password.";

        private string _usernameMessage = DefaultUsernameMessage;
        public string UsernameMessage { get { return _usernameMessage; } set { _usernameMessage = value; } }

        private string _passwordMesage = DefaultPasswordMessage;
        public string PasswordMessage { get { return _passwordMesage; } set { _passwordMesage = value; } }

        public static readonly string DefaultTargetDbOptionText = "Target database name to create or update. Defaults to the master database name detected from script names.";
        public static readonly string DefaultTargetDbServerOptionText = "Server of the target database. Defaults to localhost.";
        
        public void AddToOptionSet(OptionSet optionSet)
        {        
            optionSet.Add("targetDb=", DefaultTargetDbOptionText, arg => TargetDB = arg);
            optionSet.Add("targetDbServer=", DefaultTargetDbServerOptionText, arg => TargetDBServer = arg);
            optionSet.Add("u|username=", UsernameMessage, arg => Username = arg);
            optionSet.Add("p|password=", PasswordMessage, arg => Password = arg);
        }

        public void Validate()
        {
            if (Username == null && AuthenticationRequired)
            {
                throw new DbscOptionException("Username must be specified with -u username.");
            }

            if (Username == null && Password != null)
            {
                throw new DbscOptionException("Password but not username specified for target database.");
            }
        }

        public void PostValidate()
        {
            if (Username != null && Password == null)
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
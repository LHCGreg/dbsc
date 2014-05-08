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

        public bool UseIntegratedSecurity { get { return Username == null && IntegratedSecuritySupported; } }

        private bool _integratedSecuritySupported = false;
        /// <summary>
        /// Set to true if integrated security is supported. This will add to the help messages for username and password
        /// and not cause validation to fail if username is not specified.
        /// </summary>
        public bool IntegratedSecuritySupported { get { return _integratedSecuritySupported; } set { _integratedSecuritySupported = value; } }

        private bool _authenticationRequired = true;
        public bool AuthenticationRequired { get { return _authenticationRequired; } set { _authenticationRequired = value; } }

        private const string DefaultUsernameMessageWithoutIntegratedSecuritySupport = "Username to use to log in to the target database. REQUIRED.";
        private const string DefaultPasswordMessageWithoutIntegratedSecuritySupport = "Password to use to log in to the target database. If not specified, you will be prompted for your password.";

        private const string DefaultUsernameMessageWithIntegratedSecuritySupport = "Username to use to log in to the target database. If not specified, log in with integrated security.";
        private const string DefaultPasswordMessageWithIntegratedSecuritySupport = "Password to use to log in to the target database. If not specified and username is not specified, log in with integrated security. If not specified and username is specified, you will be prompted for your password.";

        public string UsernameMessageWithoutIntegratedSecuritySupport { get; set; }
        public string PasswordMessageWithoutIntegratedSecuritySupport { get; set; }
        public string UsernameMessageWithIntegratedSecuritySupport { get; set; }
        public string PasswordMessageWithIntegratedSecuritySupport { get; set; }

        public TargetDBOptionBundle()
        {
            UsernameMessageWithoutIntegratedSecuritySupport = DefaultUsernameMessageWithoutIntegratedSecuritySupport;
            PasswordMessageWithoutIntegratedSecuritySupport = DefaultPasswordMessageWithoutIntegratedSecuritySupport;
            UsernameMessageWithIntegratedSecuritySupport = DefaultUsernameMessageWithIntegratedSecuritySupport;
            PasswordMessageWithIntegratedSecuritySupport = DefaultPasswordMessageWithIntegratedSecuritySupport;
        }
        
        public void AddToOptionSet(OptionSet optionSet)
        {
            string usernameMessage;
            string passwordMessage;
            if (IntegratedSecuritySupported)
            {
                usernameMessage = UsernameMessageWithIntegratedSecuritySupport;
                passwordMessage = PasswordMessageWithIntegratedSecuritySupport;
            }
            else
            {
                usernameMessage = UsernameMessageWithoutIntegratedSecuritySupport;
                passwordMessage = PasswordMessageWithoutIntegratedSecuritySupport;
            }
            
            optionSet.Add("targetDb=", "Target database name to create or update. Defaults to the master database name detected from script names.", arg => TargetDB = arg);
            optionSet.Add("targetDbServer=", "Server of the target database. Defaults to localhost.", arg => TargetDBServer = arg);
            optionSet.Add("u|username=", usernameMessage, arg => Username = arg);
            optionSet.Add("p|password=", passwordMessage, arg => Password = arg);
        }

        public void Validate()
        {
            if (!IntegratedSecuritySupported && Username == null && AuthenticationRequired)
            {
                throw new DbscOptionException("Username must be specified with -u username");
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
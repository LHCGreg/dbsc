using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Options
{
    public class SourceDBOptionBundle : IOptionBundle
    {
        public string SourceDBServer { get; private set; }
        public string SourceDB { get; private set; }
        public string SourceUsername { get; private set; }
        public string SourcePassword { get; private set; }

        public bool UseIntegratedSecurity { get { return SourceUsername == null && IntegratedSecuritySupported; } }

        private bool _integratedSecuritySupported = false;
        /// <summary>
        /// Set to true if integrated security is supported. This will add to the help messages for username and password
        /// and not cause validation to fail if username is not specified.
        /// </summary>
        public bool IntegratedSecuritySupported { get { return _integratedSecuritySupported; } set { _integratedSecuritySupported = value; } }

        private bool _authenticationRequired = true;
        public bool AuthenticationRequired { get { return _authenticationRequired; } set { _authenticationRequired = value; } }

        private const string DefaultUsernameMessageWithoutIntegratedSecuritySupport = "Username to use to log in to the source database.";
        private const string DefaultPasswordMessageWithoutIntegratedSecuritySupport = "Password to use to log in to the source database. If not specified, you will be prompted for your password.";

        private const string DefaultUsernameMessageWithIntegratedSecuritySupport = "Username to use to log in to the source database. If not specified, log in with integrated security.";
        private const string DefaultPasswordMessageWithIntegratedSecuritySupport = "Password to use to log in to the source database. If not specified and username is not specified, log in with integrated security. If not specified and username is specified, you will be prompted for your password.";

        public string UsernameMessageWithoutIntegratedSecuritySupport { get; set; }
        public string PasswordMessageWithoutIntegratedSecuritySupport { get; set; }
        public string UsernameMessageWithIntegratedSecuritySupport { get; set; }
        public string PasswordMessageWithIntegratedSecuritySupport { get; set; }

        public SourceDBOptionBundle()
        {
            UsernameMessageWithoutIntegratedSecuritySupport = DefaultUsernameMessageWithoutIntegratedSecuritySupport;
            PasswordMessageWithoutIntegratedSecuritySupport = DefaultPasswordMessageWithoutIntegratedSecuritySupport;
            UsernameMessageWithIntegratedSecuritySupport = DefaultUsernameMessageWithIntegratedSecuritySupport;
            PasswordMessageWithIntegratedSecuritySupport = DefaultPasswordMessageWithIntegratedSecuritySupport;
        }

        public void AddToOptionSet(OptionSet optionSet)
        {
            string programName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            string sourceDbServerMessage = string.Format("Database server to import data from. Only specify this option if you wish to import data. Data will be imported when the target database's revision matches the source database's revision. The source database must have been created using {0}.", programName);

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

            optionSet.Add("sourceDbServer=", sourceDbServerMessage, arg => SourceDBServer = arg);
            optionSet.Add("sourceDb=", "Database to import data from. If not specified, defaults to the name used in the script file names.", arg => SourceDB = arg);
            optionSet.Add("sourceUsername=", usernameMessage, arg => SourceUsername = arg);
            optionSet.Add("sourcePassword=", passwordMessage, arg => SourcePassword = arg);
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

            if (!IntegratedSecuritySupported && SourceUsername == null && AuthenticationRequired)
            {
                throw new DbscOptionException("sourceUsername must be specified if importing data.");
            }
        }

        public void PostValidate()
        {
            if (SourceUsername != null && SourcePassword == null)
            {
                Console.Write("Password for {0} on {1}: ", SourceUsername, SourceDBServer);
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
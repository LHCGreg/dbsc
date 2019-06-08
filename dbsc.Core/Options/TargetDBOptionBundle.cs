using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;

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

        public static readonly string DefaultTargetDBOptionText = "Target database name to create or update. Defaults to the master database name detected from script names.";
        public static readonly string DefaultTargetDBServerOptionText = "Server of the target database. Defaults to localhost.";

        private string _targetDBMessage = DefaultTargetDBOptionText;
        public string TargetDBMessage { get { return _targetDBMessage; } set { _targetDBMessage = value; } }

        private string _targetDBServerMessage = DefaultTargetDBServerOptionText;
        public string TargetDBServerMessage { get { return _targetDBServerMessage; } set { _targetDBServerMessage = value; } }
        
        public void AddToOptionSet(OptionSet optionSet)
        {        
            optionSet.Add("targetDb=", TargetDBMessage, arg => TargetDB = arg);
            optionSet.Add("targetDbServer=", TargetDBServerMessage, arg => TargetDBServer = arg);
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
                Console.Write("Password for {0} on target database server {1}: ", Username, TargetDBServer);
                Password = Utils.ReadPassword();
            }
        }
    }
}

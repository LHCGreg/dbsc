using Mono.Options;
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

        private bool _authenticationRequired = true;
        public bool AuthenticationRequired { get { return _authenticationRequired; } set { _authenticationRequired = value; } }

        private const string DefaultUsernameMessage = "Username to use to log in to the source database.";
        private const string DefaultPasswordMessage = "Password to use to log in to the source database. If not specified, you will be prompted for your password.";

        private string _usernameMessage = DefaultUsernameMessage;
        public string UsernameMessage { get { return _usernameMessage; } set { _usernameMessage = value; } }

        private string _passwordMessage = DefaultPasswordMessage;
        public string PasswordMessage { get { return _passwordMessage; } set { _passwordMessage = value; } }

        public static string GetDefaultSourceDbServerOptionText()
        {
            string programName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            string sourceDbServerMessage = string.Format("Database server to import data from. Only specify this option if you wish to import data. Data will be imported when the target database's revision matches the source database's revision. The source database must have been created using {0}.", programName);
            return sourceDbServerMessage;
        }

        public static readonly string DefaultSourceDbOptionText = "Database to import data from. If not specified, defaults to the name used in the script file names.";

        public void AddToOptionSet(OptionSet optionSet)
        {
            optionSet.Add("sourceDbServer=", GetDefaultSourceDbServerOptionText(), arg => SourceDBServer = arg);
            optionSet.Add("sourceDb=", DefaultSourceDbOptionText, arg => SourceDB = arg);
            optionSet.Add("sourceUsername=", UsernameMessage, arg => SourceUsername = arg);
            optionSet.Add("sourcePassword=", PasswordMessage, arg => SourcePassword = arg);
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

            if (SourceUsername == null && AuthenticationRequired && SourceDBServer != null)
            {
                throw new DbscOptionException("sourceUsername must be specified if importing data.");
            }
        }

        public void PostValidate()
        {
            if (SourceUsername != null && SourcePassword == null)
            {
                Console.Write("Password for {0} on source database server {1}: ", SourceUsername, SourceDBServer);
                SourcePassword = Utils.ReadPassword();
            }
        }
    }
}

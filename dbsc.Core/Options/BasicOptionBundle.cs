using Mono.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Options
{
    public class BasicOptionBundle : IOptionBundle
    {
        public bool ShowHelp { get; private set; }
        public bool ShowVersion { get; private set; }

        private string m_scriptDirectory = Environment.CurrentDirectory;
        public string ScriptDirectory { get { return m_scriptDirectory; } set { m_scriptDirectory = value; } }

        public int? Revision { get; set; }

        private DbscOperation? m_operation;
        public DbscOperation Operation
        {
            get
            {
                if (m_operation.HasValue)
                    return m_operation.Value;
                else
                    throw new ArgumentNullException();
            }
            set
            {
                m_operation = value;
            }
        }

        public string ScriptDirectoryDescription { get; set; }

        public BasicOptionBundle()
        {
            ScriptDirectoryDescription = "Directory with sql scripts to run. If not specified, defaults to the current directory.";
        }
        
        public void AddToOptionSet(OptionSet optionSet)
        {
            optionSet.Add("?|h|help", "Show this message and exit.", argExistence => ShowHelp = (argExistence != null));
            optionSet.Add("v|version", "Show version information and exit.", argExistence => ShowVersion = (argExistence != null));
            optionSet.Add("dir|scriptDirectory=", ScriptDirectoryDescription, arg => ScriptDirectory = arg);
            optionSet.Add("r=", "Revision number to check out or update up to. If not specified, goes up to the highest available revision.", arg => Revision = Utils.ParseIntOption(arg, "revision"));
            optionSet.Add("<>", arg => SetCommand(arg));
        }

        private void SetCommand(string arg)
        {
            if (arg.Equals("checkout", StringComparison.OrdinalIgnoreCase))
            {
                Operation = DbscOperation.Checkout;
            }
            else if (arg.Equals("update", StringComparison.OrdinalIgnoreCase))
            {
                Operation = DbscOperation.Update;
            }
            else if (arg.Equals("revision", StringComparison.OrdinalIgnoreCase))
            {
                Operation = DbscOperation.Revision;
            }
            else
            {
                throw new DbscOptionException(string.Format("{0} is not a supported operation. Use \"checkout\" or \"update\".", arg));
            }
        }

        public void Validate()
        {
            if (m_operation == null)
            {
                throw new DbscOptionException("No operation specified.", showHelp: true);
            }
        }

        public void PostValidate()
        {
            ;
        }
    }
}

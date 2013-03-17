using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.IO;
using System.Reflection;

namespace dbsc.Core
{
    public class BaseCommandLineArgs
    {
        public bool ShowHelp { get; private set; }
        public bool ShowVersion { get; private set; }

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

        public string TargetDb { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        private string m_scriptDirectory = Environment.CurrentDirectory;
        public string ScriptDirectory { get { return m_scriptDirectory; } set { m_scriptDirectory = value; } }
        public int? Revision { get; private set; }
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

        public string SourceDbServer { get; private set; }
        public string SourceDb { get; private set; }
        public string SourceUsername { get; private set; }
        public string SourcePassword { get; private set; }
        public string ImportTableListPath { get; private set; }

        public IList<string> GetImportTableList()
        {
            if (ImportTableListPath == null)
                return null;

            string[] lines = File.ReadAllLines(ImportTableListPath);
            return lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        }

        public virtual ImportOptions GetImportOptions()
        {
            if (SourceDbServer == null)
                return null;
            
            DbConnectionInfo sourceDbInfo = new DbConnectionInfo(server: SourceDbServer, database: SourceDb, username: SourceUsername, password: SourcePassword);
            ImportOptions importOptions = new ImportOptions(sourceDbInfo);
            importOptions.TablesToImport = GetImportTableList();
            return importOptions;
        }

        public virtual OptionSet GetOptionSet()
        {
            OptionSet optionSet = new OptionSet()
            {
                { "?|h|help", "Show this message and exit.", argExistence => ShowHelp = (argExistence != null) },
                { "v|version", "Show version information and exit.", argExistence => ShowVersion = (argExistence != null) },
                { "targetDb=", "Target database name to create or update. Defaults to the master database name detected from script names.", arg => TargetDb = arg },
                // targetDbServer handled in derived classes because default is different
                { "u|username=", "Username to use to log in to the target database. If not specified, log in with integrated security.", arg => Username = arg },
                { "p|password=", "Password to use to log in to the target database. If not specified, log in with integrated security.", arg => Password = arg },
                { "dir|scriptDirectory=", "Directory with sql scripts to run. If not specified, defaults to the current directory.", arg => ScriptDirectory = arg },
                { "r=", "Revision number to check out or update up to. If not specified, goes up to the highest available revision.", arg => Revision = int.Parse(arg) }, // TODO: tryparse and throw friendly error
                { "dbCreateTemplate=", "File with a template to use when creating the database in a checkout. $DatabaseName$ will be replaced with the database name. If not specified, a simple \"CREATE DATABASE $DatabaseName$\" will be used. This is a good place to set database options or grant permissions.", arg => DbTemplateFilePath = arg },
                { "sourceDbServer=", "Database server to import data from. Data will be imported when the target database's revision matches the source database's revision. The source database must have been created using dbsc.", arg => SourceDbServer = arg },
                { "sourceDb=", "Database to import data from. If not specified, defaults to the master database name.", arg => SourceDb = arg },
                { "sourceUsername=", "Username to use to log in to the source database. If not specified, uses integrated security.", arg => SourceUsername = arg },
                { "sourcePassword=", "Password to use to log in to the source database.", arg => SourcePassword = arg },
                { "importTableList=", "File with a list of tables to import from the source database, one per line. If not specified, all tables will be imported.", arg => ImportTableListPath = arg },
                { "<>", arg => SetCommand(arg) }
            };

            return optionSet;
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
            else
            {
                throw new OptionException(string.Format("{0} is not a supported operation. Use \"checkout\" or \"update\".", arg), "<>");
            }
        }

        public BaseCommandLineArgs()
        {
            ;
        }

        public void Parse(string[] args)
        {
            OptionSet optionSet = GetOptionSet();
            optionSet.Parse(args);

            AfterParse(args);
        }

        /// <summary>
        /// Derived classes should call the base class's method first.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void AfterParse(string[] args)
        {
            if (ShowVersion)
            {
                Console.WriteLine("{0} {1}", GetProgramNameWithoutExtension(), GetVersion());
            }
            if (ShowHelp)
            {
                DisplayHelp(Console.Out);
            }

            if (ShowHelp || ShowVersion)
            {
                Environment.Exit(0);
            }

            if (m_operation == null)
            {
                throw new OptionException(string.Format("No operation specified. {0} -h for help.", GetProgramNameWithExtension()), "<>");
            }

            if (SourceDb != null && SourceDbServer == null)
            {
                throw new OptionException("sourceDbServer must be specified if importing data.", "sourceDb");
            }

            if (SourceUsername != null && SourceDbServer == null)
            {
                throw new OptionException("sourceDbServer must be specified if importing data.", "sourceUsername");
            }

            if (SourcePassword != null && SourceDbServer == null)
            {
                throw new OptionException("sourceDbServer must be specified if importing data.", "sourcePassword");
            }
        }

        public void DisplayHelp(TextWriter writer)
        {
            writer.WriteLine("Usage: {0} <command> [OPTIONS]", GetProgramNameWithExtension());
            writer.WriteLine("Available commands are \"checkout\" and \"update\".");
            writer.WriteLine();
            writer.WriteLine("Parameters:");
            GetOptionSet().WriteOptionDescriptions(writer);
        }

        public static string GetProgramNameWithoutExtension()
        {
            string[] argsWithProgramName = System.Environment.GetCommandLineArgs();
            string programName;
            if (argsWithProgramName[0].Equals(string.Empty))
            {
                // "If the file name is not available, the first element is equal to String.Empty."
                // Doesn't say why that would happen, but ok...
                programName = (new AssemblyName(Assembly.GetEntryAssembly().FullName).Name);
            }
            else
            {
                programName = Path.GetFileNameWithoutExtension(argsWithProgramName[0]);
            }

            return programName;
        }

        public static string GetProgramNameWithExtension()
        {
            string[] argsWithProgramName = System.Environment.GetCommandLineArgs();
            string programName;
            if (argsWithProgramName[0].Equals(string.Empty))
            {
                // "If the file name is not available, the first element is equal to String.Empty."
                // Doesn't say why that would happen, but ok...
                programName = (new AssemblyName(Assembly.GetEntryAssembly().FullName).Name) + ".exe";
            }
            else
            {
                programName = Path.GetFileName(argsWithProgramName[0]);
            }

            return programName;
        }

        public static string GetVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Mono.Options;

namespace dbsc.Core.Options
{
    /// <summary>
    /// Base class for processing command line args. Includes basic options that are common to all flavors.
    /// Each dbsc flavor creates a class deriving from this class, adding the options specific to that flavor.
    /// </summary>
    public class BaseCommandLineArgs
    {
        public BasicOptionBundle BasicOptions { get; private set; }

        public string ScriptDirectory { get { return BasicOptions.ScriptDirectory; } }
        public int? Revison { get { return BasicOptions.Revision; } }
        public DbscOperation Operation { get { return BasicOptions.Operation; } }

        /// <summary>
        /// Set or add to in derived classes to add options. BasicOptionBundle is always parsed and is not part of ExtraOptions.
        /// </summary>
        protected IList<IOptionBundle> ExtraOptions { get; set; }

        public BaseCommandLineArgs()
        {
            BasicOptions = new BasicOptionBundle();
            ExtraOptions = new List<IOptionBundle>();
        }

        public void Parse(string[] args)
        {
            OptionSet optionSet = GetOptionSet();
            optionSet.Parse(args);

            AfterParse(args);
        }

        private OptionSet GetOptionSet()
        {
            OptionSet optionSet = new OptionSet();
            BasicOptions.AddToOptionSet(optionSet);
            foreach(IOptionBundle extraOptionBundle in ExtraOptions)
            {
                extraOptionBundle.AddToOptionSet(optionSet);
            }
            return optionSet;
        }

        private void AfterParse(string[] args)
        {
            if (BasicOptions.ShowVersion)
            {
                Console.WriteLine("{0} {1}", GetProgramNameWithoutExtension(), GetVersion());
            }
            if (BasicOptions.ShowHelp)
            {
                DisplayHelp(Console.Out);
            }

            if (BasicOptions.ShowHelp || BasicOptions.ShowVersion)
            {
                Environment.Exit(0);
            }

            BasicOptions.Validate();
            foreach (IOptionBundle optionBundle in ExtraOptions)
            {
                optionBundle.Validate();
            }

            BasicOptions.PostValidate();
            foreach (IOptionBundle optionBundle in ExtraOptions)
            {
                optionBundle.PostValidate();
            }
        }

        public void DisplayHelp(TextWriter writer)
        {
            writer.WriteLine("Usage: {0} <command> [OPTIONS]", GetProgramNameWithoutExtension());
            writer.WriteLine("Available commands are \"checkout\", \"update\", and \"revision\".");
            writer.WriteLine();
            writer.WriteLine("Parameters:");
            GetOptionSet().WriteOptionDescriptions(writer);
        }

        private static string GetProgramNameWithoutExtension()
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

        private static string GetVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
    }
}

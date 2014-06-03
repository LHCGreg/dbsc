using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using NDesk.Options;

namespace dbsc.Core.Options
{
    /// <summary>
    /// Base class for processing command line args. Includes basic options that are common to all flavors.
    /// Each dbsc flavor creates a class deriving from this class, adding the options specific to that flavor.
    /// </summary>
    public class BaseCommandLineArgs
    {
        private BasicOptionBundle BasicOptions { get; set; }

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Options;

namespace dbsc.Core.Options
{
    public class ImportTableListFileOptionBundle : IOptionBundle
    {
        public string ImportTableListPath { get; private set; }
        public IList<string> TablesToImport { get; private set; }

        /// <summary>
        /// Set this to change the message when displaying command line help.
        /// </summary>
        public string HelpMessage { get; set; }

        public ImportTableListFileOptionBundle()
        {
            HelpMessage = "File with a list of tables to import from the source database, one per line. If not specified, all tables will be imported.";
        }

        public void AddToOptionSet(OptionSet optionSet)
        {
            optionSet.Add("importTableList=", HelpMessage, arg => ImportTableListPath = arg);
        }

        public void Validate()
        {
            ;
        }

        public void PostValidate()
        {
            if (ImportTableListPath != null)
            {
                try
                {
                    TablesToImport = File.ReadLines(ImportTableListPath).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
                }
                catch (Exception ex)
                {
                    throw new DbscOptionException(string.Format("Error reading import table list {0}: {1}", ImportTableListPath, ex.Message), ex);
                }
            }
        }
    }
}

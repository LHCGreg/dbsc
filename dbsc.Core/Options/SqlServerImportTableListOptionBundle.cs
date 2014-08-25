using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.ImportTableSpecification;
using dbsc.Core.Options;
using NDesk.Options;

namespace dbsc.Core.Options
{
    public class ImportTableListOptionBundle<TParseResult> : IOptionBundle
    {
        public string ImportTableListPath { get; private set; }
        public TParseResult ImportTableSpecifications { get; private set; }

        public string OptionDescription { get; set; }

        public static readonly string WildcardsNegationsAndCustomSelectDescription = "File with a list of tables to import from the source database, one per line. Wildcards (*) may be used. A table specification may be prefixed with a minus sign (-) to exclude the table or tables matched. If a table matches multiple lines, some of which are includes and others excludes, the last line to match wins. If the file consists only of exclusions, then a table not matching any specification will be imported. Otherwise a table that does not match any inclusion rules is not imported. A custom SELECT statement may be specified by adding \": SELECT foo, bar FROM baz\" at the end of a line. If this parameter is not specified, all tables will be imported.";
        public static readonly string WildcardsAndNegationsDescription = "File with a list of tables to import from the source database, one per line. Wildcards (*) may be used. A table specification may be prefixed with a minus sign (-) to exclude the table or tables matched. If a table matches multiple lines, some of which are includes and others excludes, the last line to match wins. If the file consists only of exclusions, then a table not matching any specification will be imported. Otherwise a table that does not match any inclusion rules is not imported. If this parameter is not specified, all tables will be imported.";

        private IImportTableListParser<TParseResult> _parser;

        public ImportTableListOptionBundle(IImportTableListParser<TParseResult> parser, string optionDescription)
        {
            _parser = parser;
            OptionDescription = optionDescription;
        }

        public void AddToOptionSet(OptionSet optionSet)
        {
            optionSet.Add("importTableList=", OptionDescription, arg => ImportTableListPath = arg);
        }

        public void Validate()
        {

        }

        public void PostValidate()
        {
            if (ImportTableListPath != null)
            {
                try
                {
                    using (TextReader reader = new StreamReader(ImportTableListPath))
                    {
                        ImportTableSpecifications = _parser.Parse(reader, Path.GetFileName(ImportTableListPath));
                    }
                }
                catch (TableSpecificationParseException)
                {
                    // The exception already contains the error message to display.
                    throw;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error reading import table list file {0}: {1}", ImportTableListPath, ex.Message), ex);
                }
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
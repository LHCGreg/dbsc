using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.ImportTableSpecification;
using dbsc.Core.Options;
using Mono.Options;

namespace dbsc.Core.Options
{
    public class ImportTableListOptionBundle<TParseResult> : IOptionBundle
    {
        public string ImportTableListPath { get; private set; }
        public TParseResult ImportTableSpecifications { get; private set; }

        public string OptionDescription { get; set; }

        public static readonly string WildcardsNegationsAndCustomSelectDescription = "File with a list of tables to import from the source database, one per line. Wildcards (*) may be used. A table specification may be prefixed with a minus sign (-) to exclude the table or tables matched. If a table matches multiple lines, some of which are includes and others excludes, the last line to match wins. If the file consists only of exclusions, then a table not matching any specification will be imported. Otherwise a table that does not match any inclusion rules is not imported. A custom SELECT statement may be specified by adding \": SELECT foo, bar FROM baz\" at the end of a line. If this parameter is not specified, all tables will be imported.";
        public static readonly string WildcardsAndNegationsDescription = "File with a list of tables to import from the source database, one per line. Wildcards (*) may be used. A table specification may be prefixed with a minus sign (-) to exclude the table or tables matched. If a table matches multiple lines, some of which are includes and others excludes, the last line to match wins. If the file consists only of exclusions, then a table not matching any specification will be imported. Otherwise a table that does not match any inclusion rules is not imported. If this parameter is not specified, all tables will be imported.";
        public static readonly string MongoWildcardsAndNegationsDescription = "File with a list of collections to import from the source database, one per line. Wildcards (*) may be used. A collection specification may be prefixed with a minus sign (-) to exclude the collection or collections matched. If a collection matches multiple lines, some of which are includes and others excludes, the last line to match wins. If the file consists only of exclusions, then a collection not matching any specification will be imported. Otherwise a collection that does not match any inclusion rules is not imported. If this parameter is not specified, all collections will be imported.";

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

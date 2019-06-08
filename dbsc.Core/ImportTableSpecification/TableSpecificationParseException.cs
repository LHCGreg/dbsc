using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core;

namespace dbsc.Core.ImportTableSpecification
{
    public class TableSpecificationParseException : DbscException
    {
        public IList<string> Errors { get; private set; }

        public TableSpecificationParseException(IList<string> errors)
            : base("Syntax errors in import table specification file:"+Environment.NewLine+string.Join(Environment.NewLine, errors))
        {
            Errors = errors;
        }

        public TableSpecificationParseException(string error)
            : this(new List<string>(1) { error })
        {

        }
    }
}

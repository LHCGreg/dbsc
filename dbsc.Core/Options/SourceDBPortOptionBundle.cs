using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;

namespace dbsc.Core.Options
{
    public class SourceDBPortOptionBundle : IOptionBundle
    {
        public int? SourceDBPort { get; private set; }
        
        public void AddToOptionSet(OptionSet optionSet)
        {
            optionSet.Add("sourcePort|sourceDbPort=", "Port number of the source database to connect to. Defaults to the normal port.", arg => SourceDBPort = Utils.ParseIntOption(arg, "sourcePort"));
        }

        public void Validate()
        {
            ;
        }

        public void PostValidate()
        {
            ;
        }
    }
}

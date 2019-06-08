using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;

namespace dbsc.Core.Options
{
    public class TargetDBPortOptionBundle : IOptionBundle
    {
        public int? TargetDBPort { get; private set; }
        
        public void AddToOptionSet(OptionSet optionSet)
        {
            optionSet.Add("port|targetDbPort=", "Port number of the target database to connect to. Defaults to the normal port.", arg => TargetDBPort = Utils.ParseIntOption(arg, "port"));
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

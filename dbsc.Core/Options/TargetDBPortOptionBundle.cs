using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;

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
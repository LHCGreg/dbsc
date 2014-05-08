using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Options
{
    /// <summary>
    /// Bundles command line options together. dbsc flavors can mix in the bundles they need to avoid duplication
    /// without including options that are not relevant for a particular flavor.
    /// </summary>
    public interface IOptionBundle
    {
        /// <summary>
        /// Adds the options to the OptionSet.
        /// </summary>
        /// <param name="optionSet"></param>
        void AddToOptionSet(OptionSet optionSet);

        /// <summary>
        /// Throws a DbscOptionException if there's an error.
        /// </summary>
        void Validate();

        void PostValidate();
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
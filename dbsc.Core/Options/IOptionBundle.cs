using Mono.Options;
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

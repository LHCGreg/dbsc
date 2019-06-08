using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface IConnectionSettings
    {
        /// <summary>
        /// Returns a string that could be used in a phrase like "Source database X was not created with dbsc and cannot be imported from".
        /// </summary>
        /// <returns></returns>
        string ToDescriptionString();

        string Database { get; set; }
    }
}

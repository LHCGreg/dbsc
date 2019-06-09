using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Sql
{
    /// <summary>
    /// Settings used for checking out a SQL database.
    /// </summary>
    /// <typeparam name="TConnectionSettings"></typeparam>
    /// <typeparam name="TImportSettings"></typeparam>
    /// <typeparam name="TUpdateSettings"></typeparam>
    public interface ISqlCheckoutSettings<TConnectionSettings, TImportSettings, TUpdateSettings>
        : ICheckoutOptions<TConnectionSettings, TImportSettings, TUpdateSettings>
        where TUpdateSettings : ISqlUpdateSettings<TConnectionSettings, TImportSettings>
    {
        string CreationTemplate { get; set; }
    }
}

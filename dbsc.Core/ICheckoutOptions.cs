using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface ICheckoutOptions<TConnectionSettings, TImportSettings>
    {
        string Directory { get; set; }
        TConnectionSettings TargetDatabase { get; set; }
        int? Revision { get; set; }
        TImportSettings ImportOptions { get; set; }
    }
    
    /// <summary>
    /// Generic interface for settings needed to check out a database.
    /// </summary>
    /// <typeparam name="TConnectionSettings"></typeparam>
    /// <typeparam name="TImportSettings"></typeparam>
    /// <typeparam name="TUpdateOptions"></typeparam>
    public interface ICheckoutOptions<TConnectionSettings, TImportSettings, TUpdateOptions>
        : ICheckoutOptions<TConnectionSettings, TImportSettings>
        where TUpdateOptions : IUpdateSettings<TConnectionSettings, TImportSettings>
    {
        TUpdateOptions UpdateOptions { get; }
        ICheckoutOptions<TConnectionSettings, TImportSettings, TUpdateOptions> Clone();
    }

    public static class CheckoutOptionsExtensions
    {
        public static TCheckoutOptions CloneCheckoutOptionsWithDatabaseNamesFilledIn<TConnectionSettings, TCheckoutOptions, TImportSettings, TUpdateOptions>
            (this TCheckoutOptions options, string dbNameFromScripts)
            where TCheckoutOptions : ICheckoutOptions<TConnectionSettings, TImportSettings, TUpdateOptions>
            where TUpdateOptions : IUpdateSettings<TConnectionSettings, TImportSettings>
            where TConnectionSettings : IConnectionSettings
            where TImportSettings : IImportSettings<TConnectionSettings>
        {
            TCheckoutOptions clone = (TCheckoutOptions) options.Clone();

            if (clone.TargetDatabase.Database == null)
            {
                clone.TargetDatabase.Database = dbNameFromScripts;
            }

            if (options.ImportOptions != null && options.ImportOptions.SourceDatabase.Database == null)
            {
                clone.ImportOptions.SourceDatabase.Database = dbNameFromScripts;
            }

            return clone;
        }
    }
}

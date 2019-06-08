using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    /// <summary>
    /// Settings used for updating a database. Generic to accomodate different types of SQL databases and even non-SQL databases.
    /// </summary>
    /// <typeparam name="TConnectionSettings"></typeparam>
    /// <typeparam name="TImportSettings"></typeparam>
    public interface IUpdateSettings<TConnectionSettings, TImportSettings>
    {
        string Directory { get; set; }
        TConnectionSettings TargetDatabase { get; set; }
        int? Revision { get; set; }
        TImportSettings ImportOptions { get; set; }

        IUpdateSettings<TConnectionSettings, TImportSettings> Clone();
    }

    public static class UpdateSettingsExtensions
    {
        public static TUpdateOptions
            CloneUpdateOptionsWithDatabaseNamesFilledIn<TConnectionSettings, TImportSettings, TUpdateOptions>
            (this TUpdateOptions options, string dbNameFromScripts)
            where TConnectionSettings : IConnectionSettings
            where TUpdateOptions : IUpdateSettings<TConnectionSettings, TImportSettings>
            where TImportSettings : IImportSettings<TConnectionSettings>
        {
            TUpdateOptions clone = (TUpdateOptions)options.Clone();

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

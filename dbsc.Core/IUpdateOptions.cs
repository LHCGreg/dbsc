using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface IUpdateOptions
    {
        string Directory { get; set; }
        DbConnectionInfo TargetDatabase { get; set; }
        int? Revision { get; set; }
        ImportOptions ImportOptions { get; set; }

        IUpdateOptions Clone();
    }

    public static class UpdateOptionsExtensions
    {
        public static TUpdateOptions CloneUpdateOptionsWithDatabaseNamesFilledIn<TUpdateOptions>(this TUpdateOptions options, string dbNameFromScripts)
            where TUpdateOptions : IUpdateOptions
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

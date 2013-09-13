using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface ICheckoutOptions<TUpdateOptions>
        where TUpdateOptions : IUpdateOptions
    {
        string Directory { get; set; }
        DbConnectionInfo TargetDatabase { get; set; }
        int? Revision { get; set; }
        ImportOptions ImportOptions { get; set; }

        TUpdateOptions UpdateOptions { get; }
        ICheckoutOptions<TUpdateOptions> Clone();
    }

    public static class CheckoutOptionsExtensions
    {
        public static TCheckoutOptions CloneCheckoutOptionsWithDatabaseNamesFilledIn<TCheckoutOptions, TUpdateOptions>(this TCheckoutOptions options, string dbNameFromScripts)
            where TCheckoutOptions : ICheckoutOptions<TUpdateOptions>
            where TUpdateOptions : IUpdateOptions
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

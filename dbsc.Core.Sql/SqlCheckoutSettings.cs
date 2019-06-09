using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Sql
{
    /// <summary>
    /// Typical settings needed for checking out a SQL database.
    /// </summary>
    public class SqlCheckoutSettings : ISqlCheckoutSettings<DbConnectionInfo, ImportSettingsWithTableList<DbConnectionInfo>, SqlUpdateSettings>
    {
        public string Directory { get; set; }
        public DbConnectionInfo TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public string CreationTemplate { get; set; }
        public ImportSettingsWithTableList<DbConnectionInfo> ImportOptions { get; set; }

        public SqlCheckoutSettings(DbConnectionInfo targetDatabase)
        {
            TargetDatabase = targetDatabase;
            Directory = Environment.CurrentDirectory;
            CreationTemplate = dbsc.Core.Options.DBCreateTemplateOptionBundle.DefaultSQLTemplate;
        }

        public SqlUpdateSettings UpdateOptions
        {
            get
            {
                return new SqlUpdateSettings(this);
            }
        }

        public SqlCheckoutSettings Clone()
        {
            SqlCheckoutSettings clone = new SqlCheckoutSettings(TargetDatabase.Clone());
            clone.Directory = Directory;
            clone.Revision = Revision;
            clone.CreationTemplate = CreationTemplate;

            if (ImportOptions != null)
            {
                clone.ImportOptions = ImportOptions.Clone();
            }

            return clone;
        }

        ICheckoutOptions<DbConnectionInfo, ImportSettingsWithTableList<DbConnectionInfo>, SqlUpdateSettings> ICheckoutOptions<DbConnectionInfo, ImportSettingsWithTableList<DbConnectionInfo>, SqlUpdateSettings>.Clone()
        {
            return Clone();
        }
    }
}

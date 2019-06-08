using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    /// <summary>
    /// Typical settings needed for importing data.
    /// </summary>
    public class ImportSettingsWithTableList<TConnectionSettings> : IImportSettingsWithTableList<TConnectionSettings>, ICloneable
        where TConnectionSettings : ICloneable
    {
        public TConnectionSettings SourceDatabase { get; set; }
        public IList<string> TablesToImport { get; set; }

        public ImportSettingsWithTableList(TConnectionSettings sourceDatabase)
        {
            SourceDatabase = sourceDatabase;
        }

        public ImportSettingsWithTableList<TConnectionSettings> Clone()
        {
            ImportSettingsWithTableList<TConnectionSettings> clone = new ImportSettingsWithTableList<TConnectionSettings>((TConnectionSettings) SourceDatabase.Clone());

            if (TablesToImport != null)
            {
                clone.TablesToImport = new List<string>(TablesToImport);
            }

            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}

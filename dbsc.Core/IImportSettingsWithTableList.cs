using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface IImportSettingsWithTableList<TConnectionSettings> : IImportSettings<TConnectionSettings>
    {
        IList<string> TablesToImport { get; }
    }
}

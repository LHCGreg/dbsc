using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface IDbscEngineWithTableImport<TConnectionSettings, TImportSettings, TUpdateSettings>
        where TImportSettings : IImportSettingsWithTableList<TConnectionSettings>
        where TUpdateSettings : IUpdateSettings<TConnectionSettings, TImportSettings>
    {
        ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(TConnectionSettings connectionSettings);
        void ImportData(TUpdateSettings updateSettings, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadata);
    }

    public static class DbscEngineWithTableImportExtensions
    {
        public static void ImportData<TConnectionSettings, TImportSettings, TUpdateSettings>
            (this IDbscEngineWithTableImport<TConnectionSettings, TImportSettings, TUpdateSettings> engine, TUpdateSettings updateSettings)
            where TImportSettings : IImportSettingsWithTableList<TConnectionSettings>
            where TUpdateSettings : IUpdateSettings<TConnectionSettings, TImportSettings>
        {
            Console.WriteLine("Beginning import...");
            Stopwatch timer = Stopwatch.StartNew();

            ICollection<string> tablesExceptMetadata = engine.GetTableNamesExceptMetadataAlreadyEscaped(updateSettings.TargetDatabase);

            ICollection<string> tablesToImport;
            if (updateSettings.ImportOptions.TablesToImport != null)
            {
                tablesToImport = updateSettings.ImportOptions.TablesToImport;
            }
            else
            {
                tablesToImport = tablesExceptMetadata;
            }

            engine.ImportData(updateSettings, tablesToImport, tablesExceptMetadata);

            timer.Stop();
            Console.WriteLine("Import complete! Took {0}", timer.Elapsed);
        }
    }
}

/*
 Copyright 2014 Greg Najda

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
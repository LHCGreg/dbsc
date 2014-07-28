using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    public interface IDbscEngineWithTableImport<TConnectionSettings, TImportSettings, TUpdateSettings, TTable>
        where TUpdateSettings : IUpdateSettings<TConnectionSettings, TImportSettings>
    {
        ICollection<TTable> GetTablesToImport(TUpdateSettings updateSettings);
        void ImportData(TUpdateSettings updateSettings, ICollection<TTable> tablesToImport);
    }

    public static class DbscEngineWithTableImportExtensions
    {
        public static void ImportData<TConnectionSettings, TImportSettings, TUpdateSettings, TTable>
            (this IDbscEngineWithTableImport<TConnectionSettings, TImportSettings, TUpdateSettings, TTable> engine, TUpdateSettings updateSettings)
            where TUpdateSettings : IUpdateSettings<TConnectionSettings, TImportSettings>
        {
            Console.WriteLine("Beginning import...");
            Stopwatch timer = Stopwatch.StartNew();

            ICollection<TTable> tablesToImport = engine.GetTablesToImport(updateSettings);
            engine.ImportData(updateSettings, tablesToImport);

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
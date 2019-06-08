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

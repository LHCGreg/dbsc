﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    /// <summary>
    /// Typical settings needed for importing data.
    /// </summary>
    public class ImportOptions<TConnectionSettings> : IImportSettingsWithTableList<TConnectionSettings>, ICloneable
        where TConnectionSettings : ICloneable
    {
        public TConnectionSettings SourceDatabase { get; set; }
        public IList<string> TablesToImport { get; set; }

        public ImportOptions(TConnectionSettings sourceDatabase)
        {
            SourceDatabase = sourceDatabase;
        }

        public ImportOptions<TConnectionSettings> Clone()
        {
            ImportOptions<TConnectionSettings> clone = new ImportOptions<TConnectionSettings>((TConnectionSettings) SourceDatabase.Clone());

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
﻿using System;
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

/*
 Copyright 2013 Greg Najda

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
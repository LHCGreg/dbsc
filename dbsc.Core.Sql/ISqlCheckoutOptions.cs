﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Sql
{
    /// <summary>
    /// Settings used for checking out a SQL database.
    /// </summary>
    /// <typeparam name="TConnectionSettings"></typeparam>
    /// <typeparam name="TImportSettings"></typeparam>
    /// <typeparam name="TUpdateOptions"></typeparam>
    public interface ISqlCheckoutOptions<TConnectionSettings, TImportSettings, TUpdateOptions>
        : ICheckoutOptions<TConnectionSettings, TImportSettings, TUpdateOptions>
        where TUpdateOptions : ISqlUpdateOptions<TConnectionSettings, TImportSettings>
    {
        string CreationTemplate { get; set; }
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
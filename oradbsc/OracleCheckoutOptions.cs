using dbsc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Oracle
{
    class OracleCheckoutOptions : ISqlCheckoutOptions<OracleUpdateOptions>
    {
        public string Directory { get; set; }
        public OracleDbConnectionInfo TargetDatabase { get; set; }
        public int? Revision { get; set; }
        public string CreationTemplate { get; set; }
        public OracleImportOptions ImportOptions { get; set; }

        public OracleCheckoutOptions(OracleDbConnectionInfo targetDatabase)
        {
            TargetDatabase = targetDatabase;
            Directory = Environment.CurrentDirectory;
            CreationTemplate = "CREATE DATABASE $DatabaseName$";
        }

        public OracleUpdateOptions UpdateOptions { get { return new OracleUpdateOptions(this); } }

        public OracleCheckoutOptions Clone()
        {
            OracleCheckoutOptions clone = new OracleCheckoutOptions(TargetDatabase.Clone());
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
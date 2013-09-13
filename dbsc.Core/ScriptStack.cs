using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace dbsc.Core
{
    internal class ScriptStack
    {
        public IDictionary<int, string> ScriptsByRevision { get; private set; }
        public string MasterDatabaseName { get; private set; }
        public string ExtensionWithoutDot { get; private set; }

        public ScriptStack(string directory, string extensionWithoutDot)
            : this(Directory.EnumerateFiles(directory), extensionWithoutDot)
        {
            ;
        }

        internal ScriptStack(IEnumerable<string> filePaths, string extensionWithoutDot)
        {
            ExtensionWithoutDot = extensionWithoutDot;

            ScriptsByRevision = new Dictionary<int, string>();

            // DatabaseName.#+[.Comment].sql
						Regex upgradeScriptRegex = new Regex(@"^(?<MasterDatabaseName>[^.]+)\.(?<Revision>\d+)(?<CommentWithDot>\.[^.]+)?\." + Regex.Escape(extensionWithoutDot) + "$", RegexOptions.IgnoreCase);

            foreach (string filePath in filePaths)
            {
                string filename = Path.GetFileName(filePath);

                Match m = upgradeScriptRegex.Match(filename);
                if (m.Success)
                {
                    string masterDatabaseName = m.Groups["MasterDatabaseName"].ToString();
                    string revisionString = m.Groups["Revision"].ToString();
                    int revision = int.Parse(revisionString);

                    if (MasterDatabaseName != null && !MasterDatabaseName.Equals(masterDatabaseName, StringComparison.Ordinal))
                    {
                        throw new DbscException(string.Format("Scripts for databases {0} and {1} found. Don't know which to use.", MasterDatabaseName, masterDatabaseName));
                    }
                    MasterDatabaseName = masterDatabaseName;

                    if (ScriptsByRevision.ContainsKey(revision))
                    {
                        throw new DbscException(string.Format("There is more than one script for r{0}.", revision));
                    }

                    ScriptsByRevision[revision] = filePath;
                }
            }

            if (!ScriptsByRevision.ContainsKey(0))
            {
                throw new DbscException(string.Format("No r0 script found. Syntax is MasterDatabaseName.0000[.comment].{0}.", extensionWithoutDot));
            }
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
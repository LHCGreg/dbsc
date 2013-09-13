using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace dbsc.Core
{
    public abstract class DbscEngine<TCheckoutOptions, TUpdateOptions>
        where TCheckoutOptions : ICheckoutOptions<TUpdateOptions>
        where TUpdateOptions : IUpdateOptions
    {
        protected abstract string ScriptExtensionWithoutDot { get; }
        protected abstract bool ImportIsSupported(out string whyNot);
        protected abstract bool DatabaseHasMetadataTable(DbConnectionInfo connectionInfo);
        protected abstract void CreateDatabase(TCheckoutOptions options);
        protected abstract void InitializeDatabase(TCheckoutOptions options, string masterDatabaseName);
        protected abstract int GetRevision(DbConnectionInfo connectionInfo);
        protected abstract void RunScriptAndUpdateMetadata(TUpdateOptions options, string scriptPath, int newRevision, DateTime utcTimestamp);
        protected abstract void ImportData(TUpdateOptions options, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadataAlreadyEscaped);
        protected abstract ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(DbConnectionInfo connectionInfo);
        
        public void Checkout(TCheckoutOptions options)
        {
            ScriptStack scriptStack = new ScriptStack(options.Directory, ScriptExtensionWithoutDot);

            // Default target database name and source database name to the master database name
            options = options.CloneCheckoutOptionsWithDatabaseNamesFilledIn<TCheckoutOptions, TUpdateOptions>(scriptStack.MasterDatabaseName);

            ValidateUpdateOptionsForCheckoutAndUpdate(options.UpdateOptions, scriptStack);

            CreateDatabase(options);
            InitializeDatabase(options, scriptStack.MasterDatabaseName);

            UpdateDatabase(scriptStack, options.UpdateOptions);
        }

        private void ValidateUpdateOptionsForCheckoutAndUpdate(TUpdateOptions options, ScriptStack scriptStack)
        {
            // If revision was specified, verify that there is a script for tha revision
            if (options.Revision != null && !scriptStack.ScriptsByRevision.ContainsKey(options.Revision.Value))
            {
                throw new DbscException(string.Format("Cannot update to r{0} because there is no upgrade script for r{0}.", options.Revision.Value));
            }

            if (options.ImportOptions != null)
            {
                // If user wants to do an import, check that it's supported by the engine
                string whyImportNotSupported;
                if (!ImportIsSupported(out whyImportNotSupported))
                {
                    throw new DbscException(whyImportNotSupported);
                }

                // Check that source database was checked out with dbsc
                if (!DatabaseHasMetadataTable(options.ImportOptions.SourceDatabase))
                {
                    throw new DbscException(string.Format("Source database {0} on {1} was not created with dbsc and cannot be imported from.", options.ImportOptions.SourceDatabase.Database, options.ImportOptions.SourceDatabase.Server));
                }
            }
        }

        public void Update(TUpdateOptions options)
        {
            ScriptStack scriptStack = new ScriptStack(options.Directory, ScriptExtensionWithoutDot);

            // Default target database name and source database name to the master database name
            options = options.CloneUpdateOptionsWithDatabaseNamesFilledIn(scriptStack.MasterDatabaseName);

            ValidateUpdateOptionsForCheckoutAndUpdate(options, scriptStack);
            ValidateUpdateOptionsForUpdate(options);

            UpdateDatabase(scriptStack, options);
        }

        private void ValidateUpdateOptionsForUpdate(TUpdateOptions options)
        {
            if (!DatabaseHasMetadataTable(options.TargetDatabase))
            {
                throw new DbscException(string.Format("Target database {0} on {1} was not created with dbsc and cannot be updated.", options.TargetDatabase.Database, options.TargetDatabase.Server));
            }
        }

        private void UpdateDatabase(ScriptStack scriptStack, TUpdateOptions options)
        {
            int versionBeforeUpdate = GetRevision(options.TargetDatabase);
            int currentVersion = versionBeforeUpdate;

            int sourceDatabaseRevision = -1;
            if (options.ImportOptions != null)
            {
                sourceDatabaseRevision = GetRevision(options.ImportOptions.SourceDatabase);
            }

            IEnumerable<int> revisionsToUpgradeTo = scriptStack.ScriptsByRevision.Keys.OrderBy(r => r).Where(r => r > versionBeforeUpdate);

            if (options.Revision != null)
            {
                if (versionBeforeUpdate > options.Revision.Value)
                {
                    throw new DbscException(string.Format("Cannot update to r{0} because the database is already at r{1}.", options.Revision.Value, versionBeforeUpdate));
                }
                if (!scriptStack.ScriptsByRevision.ContainsKey(options.Revision.Value))
                {
                    throw new DbscException(string.Format("Cannot update to r{0} because there is no upgrade script for r{0}.", options.Revision.Value));
                }
                revisionsToUpgradeTo = revisionsToUpgradeTo.Where(r => r <= options.Revision.Value);
            }

            revisionsToUpgradeTo = revisionsToUpgradeTo.ToList();
            foreach (int revisionNumber in revisionsToUpgradeTo)
            {
                string upgradeScriptPath = scriptStack.ScriptsByRevision[revisionNumber];
                Console.WriteLine("Updating to r{0}", revisionNumber);
                RunScriptAndUpdateMetadata(options, upgradeScriptPath, revisionNumber, DateTime.UtcNow);
                currentVersion = revisionNumber;

                // check for import
                if (options.ImportOptions != null && revisionNumber == sourceDatabaseRevision)
                {
                    ImportData(options);
                }
            }

            // allow importing when "updating" to the revison the database is already at
            if (!revisionsToUpgradeTo.Any() && versionBeforeUpdate == sourceDatabaseRevision && options.ImportOptions != null)
            {
                ImportData(options);
            }

            Console.WriteLine("At revision {0}", currentVersion);
        }

        private void ImportData(TUpdateOptions options)
        {
            Console.WriteLine("Beginning import...");
            Stopwatch timer = Stopwatch.StartNew();

            ICollection<string> tablesExceptMetadata = GetTableNamesExceptMetadataAlreadyEscaped(options.TargetDatabase);

            ICollection<string> tablesToImport;
            if (options.ImportOptions.TablesToImport != null)
            {
                tablesToImport = options.ImportOptions.TablesToImport;
            }
            else
            {
                tablesToImport = tablesExceptMetadata;
            }

            ImportData(options, tablesToImport, tablesExceptMetadata);

            timer.Stop();
            Console.WriteLine("Import complete! Took {0}", timer.Elapsed);
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
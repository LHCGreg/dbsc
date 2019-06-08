using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace dbsc.Core
{
    /// <summary>
    /// Base class containing generic, DB-agnostic code for checking out and updating databases.
    /// </summary>
    /// <typeparam name="TCheckoutSettings"></typeparam>
    /// <typeparam name="TUpdateSettings"></typeparam>
    public abstract class DbscEngine<TConnectionSettings, TCheckoutSettings, TImportSettings, TUpdateSettings>
        where TCheckoutSettings : ICheckoutOptions<TConnectionSettings, TImportSettings, TUpdateSettings>
        where TUpdateSettings : IUpdateSettings<TConnectionSettings, TImportSettings>
        where TConnectionSettings : IConnectionSettings
        where TImportSettings : IImportSettings<TConnectionSettings>
    {
        /// <summary>
        /// Extension of script files without the period. For example, "sql" for SQL scripts, "js" for javascript files.
        /// </summary>
        protected abstract string ScriptExtensionWithoutDot { get; }

        /// <summary>
        /// Returns true if checking out and updating are supported, otherwise returns false and sets <paramref name="whyNot"/>
        /// to a message explaining why not. This will normally be true but might be false if a required program is not
        /// installed, for example.
        /// </summary>
        /// <param name="whyNot"></param>
        /// <returns></returns>
        protected abstract bool CheckoutAndUpdateIsSupported(out string whyNot);

        /// <summary>
        /// Returns true if importing data from another database is supported, otherwise returns false and sets
        /// <paramref name="whyNot"/> to a message explaining why not. This might be false if a required program is not
        /// installed or if a dbsc flavor has not implemented importing or if importing does not make sense for the database type.
        /// </summary>
        /// <param name="whyNot"></param>
        /// <returns></returns>
        protected abstract bool ImportIsSupported(out string whyNot);

        /// <summary>
        /// Returns true if the database given by <paramref name="connectionInfo"/> has a dbsc metadata table.
        /// </summary>
        /// <param name="connectionInfo"></param>
        /// <returns></returns>
        protected abstract bool DatabaseHasMetadataTable(TConnectionSettings connectionInfo);

        /// <summary>
        /// Creates the database given by <paramref name="options"/>.
        /// </summary>
        /// <param name="options"></param>
        protected abstract void CreateDatabase(TCheckoutSettings options);
        
        /// <summary>
        /// Initializes the dbsc metadata table on the database.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="masterDatabaseName">The name of the database as it appears in script file names, not necessarily the
        /// name of the created database.</param>
        protected abstract void InitializeDatabase(TCheckoutSettings options, string masterDatabaseName);
        
        /// <summary>
        /// Gets the revision that the database is on.
        /// </summary>
        /// <param name="connectionInfo"></param>
        /// <returns></returns>
        protected abstract int GetRevision(TConnectionSettings connectionInfo);
        
        /// <summary>
        /// Runs the script at <paramref name="scriptPath"/> and bumps up the revision number to <paramref name="newRevision"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="scriptPath"></param>
        /// <param name="newRevision"></param>
        /// <param name="utcTimestamp"></param>
        protected abstract void RunScriptAndUpdateMetadata(TUpdateSettings options, string scriptPath, int newRevision);

        protected abstract void ImportData(TUpdateSettings options);
        
        /// <summary>
        /// Runs a database checkout.
        /// </summary>
        /// <param name="options"></param>
        public void Checkout(TCheckoutSettings options)
        {
            ScriptStack scriptStack = new ScriptStack(options.Directory, ScriptExtensionWithoutDot);

            // Default target database name and source database name to the master database name
            options = options.CloneCheckoutOptionsWithDatabaseNamesFilledIn<TConnectionSettings, TCheckoutSettings, TImportSettings, TUpdateSettings>(scriptStack.MasterDatabaseName);

            ValidateUpdateOptionsForCheckoutAndUpdate(options.UpdateOptions, scriptStack);

            CreateDatabase(options);
            InitializeDatabase(options, scriptStack.MasterDatabaseName);

            UpdateDatabase(scriptStack, options.UpdateOptions);
        }

        private void ValidateUpdateOptionsForCheckoutAndUpdate(TUpdateSettings options, ScriptStack scriptStack)
        {
            // If revision was specified, verify that there is a script for tha revision
            if (options.Revision != null && !scriptStack.ScriptsByRevision.ContainsKey(options.Revision.Value))
            {
                throw new DbscException(string.Format("Cannot update to r{0} because there is no upgrade script for r{0}.", options.Revision.Value));
            }

            string whyCheckoutAndUpdateNotSupported;
            if (!CheckoutAndUpdateIsSupported(out whyCheckoutAndUpdateNotSupported))
            {
                throw new DbscException(whyCheckoutAndUpdateNotSupported);
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
                    throw new DbscException(string.Format("Source database {0} was not created with dbsc and cannot be imported from or you do not have permission to it.", options.ImportOptions.SourceDatabase.ToDescriptionString()));
                }
            }
        }

        public void ShowRevision(TCheckoutSettings settings)
        {
            if (settings.TargetDatabase.Database == null)
            {
                ScriptStack scriptStack = new ScriptStack(settings.Directory, ScriptExtensionWithoutDot);
                // Default target database name and source database name to the master database name
                settings = settings.CloneCheckoutOptionsWithDatabaseNamesFilledIn<TConnectionSettings, TCheckoutSettings, TImportSettings, TUpdateSettings>(scriptStack.MasterDatabaseName);
            }
            int revision = GetRevision(settings.TargetDatabase);
            Console.WriteLine(revision.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Runs a database update.
        /// </summary>
        /// <param name="options"></param>
        public void Update(TUpdateSettings options)
        {
            ScriptStack scriptStack = new ScriptStack(options.Directory, ScriptExtensionWithoutDot);

            // Default target database name and source database name to the master database name
            options = options.CloneUpdateOptionsWithDatabaseNamesFilledIn<TConnectionSettings, TImportSettings, TUpdateSettings>(scriptStack.MasterDatabaseName);

            ValidateUpdateOptionsForCheckoutAndUpdate(options, scriptStack);
            ValidateUpdateOptionsForUpdate(options);

            UpdateDatabase(scriptStack, options);
        }

        private void ValidateUpdateOptionsForUpdate(TUpdateSettings options)
        {
            if (!DatabaseHasMetadataTable(options.TargetDatabase))
            {
                throw new DbscException(string.Format("Target database {0} was not created with dbsc and cannot be updated.", options.TargetDatabase.ToDescriptionString()));
            }
        }

        private void UpdateDatabase(ScriptStack scriptStack, TUpdateSettings options)
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
                RunScriptAndUpdateMetadata(options, upgradeScriptPath, revisionNumber);
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
    }
}

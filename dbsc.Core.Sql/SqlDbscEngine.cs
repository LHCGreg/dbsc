﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace dbsc.Core.Sql
{
    /// <summary>
    /// Implements common behavior for SQL databases by filling in some of the required behavior of a DbscEngine.
    /// Some holes are left for the implementation for a specific database to fill in in the form of abstract methods.
    /// </summary>
    /// <typeparam name="TConnectionSettings"></typeparam>
    /// <typeparam name="TCheckoutOptions"></typeparam>
    /// <typeparam name="TImportSettings"></typeparam>
    /// <typeparam name="TUpdateSettings"></typeparam>
    /// <typeparam name="TConnection"></typeparam>
    public abstract class SqlDbscEngine<TConnectionSettings, TCheckoutOptions, TImportSettings, TUpdateSettings, TConnection, TTable>
        : DbscEngine<TConnectionSettings, TCheckoutOptions, TImportSettings, TUpdateSettings>
        , IDbscEngineWithTableImport<TConnectionSettings, TImportSettings, TUpdateSettings, TTable>
        where TCheckoutOptions : ISqlCheckoutSettings<TConnectionSettings, TImportSettings, TUpdateSettings>
        where TUpdateSettings : ISqlUpdateSettings<TConnectionSettings, TImportSettings>
        where TConnectionSettings : IConnectionSettings
        where TImportSettings : IImportSettings<TConnectionSettings>
        where TConnection : IDbscDbConnection
    {
        /// <summary>
        /// Opens a database connection.
        /// </summary>
        /// <param name="connectionInfo"></param>
        /// <returns></returns>
        protected abstract TConnection OpenConnection(TConnectionSettings connectionInfo);

        /// <summary>
        /// Returns true if the dbsc metadata table exists.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        protected abstract bool MetadataTableExists(TConnection conn);

        /// <summary>
        /// Returns connection settings for the database to connect to before the database we're creating exists.
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        protected abstract TConnectionSettings GetSystemDatabaseConnectionInfo(TConnectionSettings database);

        /// <summary>
        /// SQL for creating the dbsc metadata table.
        /// </summary>
        protected abstract string CreateMetadataTableSql { get; }

        /// <summary>
        /// Imports data from another database into the database being updated
        /// </summary>
        /// <param name="updateSettings"></param>
        /// <param name="tablesToImport"></param>
        public abstract void ImportData(TUpdateSettings updateSettings, ICollection<TTable> tablesToImport);

        /// <summary>
        /// Returns the tables that should be imported.
        /// </summary>
        /// <param name="updateSettings"></param>
        /// <returns></returns>
        public abstract ICollection<TTable> GetTablesToImport(TUpdateSettings updateSettings);

        protected override string ScriptExtensionWithoutDot { get { return "sql"; } }

        protected virtual string MetadataTableName { get { return "dbsc_metadata"; } }
        protected virtual string MetadataPropertyNameColumn { get { return "property_name"; } }
        protected virtual string MetadataPropertyValueColumn { get { return "property_value"; } }
        
        /// <summary>
        /// Character used before parameters in queries. @ for most databases.
        /// </summary>
        protected abstract char QueryParamChar { get; }

        protected override bool DatabaseHasMetadataTable(TConnectionSettings connectionInfo)
        {
            using (TConnection conn = OpenConnection(connectionInfo))
            {
                return MetadataTableExists(conn);
            }
        }

        protected override void CreateDatabase(TCheckoutOptions options)
        {
            TConnectionSettings masterDatabaseConnectionInfo = GetSystemDatabaseConnectionInfo(options.TargetDatabase);
            Console.WriteLine("Creating database {0}.", options.TargetDatabase.ToDescriptionString());
            using (TConnection masterDatabaseConnection = OpenConnection(masterDatabaseConnectionInfo))
            {
                string creationSql = options.CreationTemplate.Replace("$DatabaseName$", options.TargetDatabase.Database);
                masterDatabaseConnection.ExecuteSqlScript(creationSql);
                Console.WriteLine("Created database {0}.", options.TargetDatabase.ToDescriptionString());
            }
        }

        protected string MasterDatabasePropertyName { get { return "MasterDatabaseName"; } }
        protected string RevisionPropertyName { get { return "Version"; } }
        protected string LastUpdatedPropertyName { get { return "LastChangeUTC"; } }

        protected override void InitializeDatabase(TCheckoutOptions options, string masterDatabaseName)
        {
            using (TConnection conn = OpenConnection(options.TargetDatabase))
            {
                conn.ExecuteSql(CreateMetadataTableSql);

                Dictionary<string, string> initialProperties = new Dictionary<string, string>()
                {
                    { MasterDatabasePropertyName, masterDatabaseName },
                    { RevisionPropertyName, "-1" },
                    { LastUpdatedPropertyName, GetCurrentTimestampString() }
                };

                CreateMetadataProperties(conn, initialProperties);
            }
        }

        protected string GetCurrentTimestampString()
        {
            return GetTimestampString(DateTime.UtcNow);
        }

        private string GetTimestampString(DateTime timestamp)
        {
            return timestamp.ToString("s"); // ex: 2008-04-10T06:30:00
        }

        protected virtual void CreateMetadataProperties(TConnection conn, IDictionary<string, string> properties)
        {
            if (properties.Count == 0)
                return;

            StringBuilder sqlBuilder = new StringBuilder(string.Format(@"INSERT INTO {0}
({1}, {2})
VALUES
", MetadataTableName, MetadataPropertyNameColumn, MetadataPropertyValueColumn));
            int propertyNum = 0;
            Dictionary<string, object> sqlParams = new Dictionary<string, object>();
            foreach (string propertyName in properties.Keys)
            {
                if (propertyNum > 0)
                {
                    sqlBuilder.Append(",");
                }
                sqlBuilder.AppendFormat("({0}name{1}, {0}value{1})\n", QueryParamChar, propertyNum);
                sqlParams["name" + propertyNum.ToString(CultureInfo.InvariantCulture)] = propertyName;
                sqlParams["value" + propertyNum.ToString(CultureInfo.InvariantCulture)] = properties[propertyName];
                propertyNum++;
            }

            string sql = sqlBuilder.ToString();
            conn.ExecuteSql(sql, sqlParams);
        }

        protected virtual void UpdateMetadataProperty(TConnection conn, string propertyName, string propertyValue)
        {
            string sql = string.Format(@"UPDATE {0}
SET {2} = {1}value
WHERE {3} = {1}name", MetadataTableName, QueryParamChar, MetadataPropertyValueColumn, MetadataPropertyNameColumn);

            Dictionary<string, object> sqlParams = new Dictionary<string, object>();
            sqlParams["value"] = propertyValue;
            sqlParams["name"] = propertyName;

            conn.ExecuteSql(sql, sqlParams);
        }

        protected virtual string GetMetadataProperty(TConnection conn, string propertyName)
        {
            string sql = string.Format(@"SELECT {0} FROM {1}
WHERE {2} = {3}name", MetadataPropertyValueColumn, MetadataTableName, MetadataPropertyNameColumn, QueryParamChar);
            Dictionary<string, object> sqlParams = new Dictionary<string, object>() { { "name", propertyName } };

            string propertyValue = conn.Query<string>(sql, sqlParams).FirstOrDefault();

            if (propertyValue == null)
            {
                throw new DbscException(string.Format("error: No metadata property {0}.", propertyName));
            }
            else
            {
                return propertyValue;
            }
        }

        protected override int GetRevision(TConnectionSettings connectionInfo)
        {
            using (TConnection conn = OpenConnection(connectionInfo))
            {
                string revisionString = GetMetadataProperty(conn, RevisionPropertyName);
                int revision;
                if (!int.TryParse(revisionString, out revision))
                {
                    throw new DbscException(string.Format("error: {0} metadata property on database {1} has value \"{2}\" is not an integer.",
                        RevisionPropertyName, connectionInfo.ToDescriptionString(), revisionString));
                }
                return revision;
            }
        }

        protected override void RunScriptAndUpdateMetadata(TUpdateSettings options, string scriptPath, int newRevision)
        {
            string scriptText = File.ReadAllText(scriptPath);
            using (TConnection conn = OpenConnection(options.TargetDatabase))
            {
                conn.ExecuteSqlScript(scriptText);

                string newRevisionString = newRevision.ToString(CultureInfo.InvariantCulture);
                UpdateMetadataProperty(conn, RevisionPropertyName, newRevisionString);

                string utcTimestampString = GetCurrentTimestampString();
                UpdateMetadataProperty(conn, LastUpdatedPropertyName, utcTimestampString);
            }
        }

        protected override void ImportData(TUpdateSettings options)
        {
            DbscEngineWithTableImportExtensions.ImportData(this, options);
        }
    }
}

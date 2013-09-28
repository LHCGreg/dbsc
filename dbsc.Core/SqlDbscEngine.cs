using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace dbsc.Core
{
    public abstract class SqlDbscEngine<TCheckoutOptions, TUpdateOptions, TConnection> : DbscEngine<TCheckoutOptions, TUpdateOptions>
        where TCheckoutOptions : ISqlCheckoutOptions<TUpdateOptions>
        where TUpdateOptions : ISqlUpdateOptions
        where TConnection : IDbscDbConnection
    {
        protected abstract TConnection OpenConnection(DbConnectionInfo connectionInfo);
        protected abstract bool MetadataTableExists(TConnection conn);
        protected abstract DbConnectionInfo GetSystemDatabaseConnectionInfo(DbConnectionInfo database);
        protected abstract string CreateMetadataTableSql { get; }
        protected abstract ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(TConnection connectionInfo);

        protected override string ScriptExtensionWithoutDot { get { return "sql"; } }

        protected virtual string MetadataTableName { get { return "dbsc_metadata"; } }
        protected virtual string MetadataPropertyNameColumn { get { return "property_name"; } }
        protected virtual string MetadataPropertyValueColumn { get { return "property_value"; } }

        protected override bool DatabaseHasMetadataTable(DbConnectionInfo connectionInfo)
        {
            using (TConnection conn = OpenConnection(connectionInfo))
            {
                return MetadataTableExists(conn);
            }
        }

        protected override void CreateDatabase(TCheckoutOptions options)
        {
            DbConnectionInfo masterDatabaseConnectionInfo = GetSystemDatabaseConnectionInfo(options.TargetDatabase);
            using (TConnection masterDatabaseConnection = OpenConnection(masterDatabaseConnectionInfo))
            {
                string creationSql = options.CreationTemplate.Replace("$DatabaseName$", options.TargetDatabase.Database);
                Console.WriteLine("Creating database {0} on {1}.", options.TargetDatabase.Database, options.TargetDatabase.Server);
                masterDatabaseConnection.ExecuteSqlScript(creationSql);
                Console.WriteLine("Created database {0} on {1}.", options.TargetDatabase.Database, options.TargetDatabase.Server);
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

        private string GetCurrentTimestampString()
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
                sqlBuilder.AppendFormat("(@name{0}, @value{0})\n", propertyNum);
                sqlParams["name" + propertyNum.ToString()] = propertyName;
                sqlParams["value" + propertyNum.ToString()] = properties[propertyName];
                propertyNum++;
            }

            string sql = sqlBuilder.ToString();
            conn.ExecuteSql(sql, sqlParams);
        }

        protected virtual void UpdateMetadataProperty(TConnection conn, string propertyName, string propertyValue)
        {
            string sql = string.Format(@"UPDATE {0}
SET {1} = @value
WHERE {2} = @name", MetadataTableName, MetadataPropertyValueColumn, MetadataPropertyNameColumn);

            Dictionary<string, object> sqlParams = new Dictionary<string, object>();
            sqlParams["value"] = propertyValue;
            sqlParams["name"] = propertyName;

            conn.ExecuteSql(sql, sqlParams);
        }

        protected virtual string GetMetadataProperty(TConnection conn, string propertyName)
        {
            string sql = string.Format(@"SELECT {0} FROM {1}
WHERE {2} = @name", MetadataPropertyValueColumn, MetadataTableName, MetadataPropertyNameColumn);
            Dictionary<string, object> sqlParams = new Dictionary<string, object>() { { "name", propertyName } };

            string propertyValue = conn.Query<string>(sql, sqlParams).FirstOrDefault();

            if (propertyValue == null)
            {
                throw new DbscException(string.Format("No metadata property {0}.", propertyName));
            }
            else
            {
                return propertyValue;
            }
        }

        protected override int GetRevision(DbConnectionInfo connectionInfo)
        {
            using (TConnection conn = OpenConnection(connectionInfo))
            {
                string revisionString = GetMetadataProperty(conn, RevisionPropertyName);
                int revision;
                if (!int.TryParse(revisionString, out revision))
                {
                    throw new DbscException(string.Format("{0} metadata property on database {1} on {2} has value \"{3}\" is not an integer.",
                        RevisionPropertyName, connectionInfo.Database, connectionInfo.Server, revisionString));
                }
                return revision;
            }
        }

        protected override void RunScriptAndUpdateMetadata(TUpdateOptions options, string scriptPath, int newRevision, DateTime utcTimestamp)
        {
            string scriptText = File.ReadAllText(scriptPath);
            using (TConnection conn = OpenConnection(options.TargetDatabase))
            {
                conn.ExecuteSqlScript(scriptText);

                string newRevisionString = newRevision.ToString(CultureInfo.InvariantCulture);
                UpdateMetadataProperty(conn, RevisionPropertyName, newRevision.ToString(CultureInfo.InvariantCulture));

                string utcTimestampString = GetTimestampString(utcTimestamp);
                UpdateMetadataProperty(conn, LastUpdatedPropertyName, utcTimestampString);
            }
        }

        protected override ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(DbConnectionInfo connectionInfo)
        {
            using (TConnection conn = OpenConnection(connectionInfo))
            {
                return GetTableNamesExceptMetadataAlreadyEscaped(conn);
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
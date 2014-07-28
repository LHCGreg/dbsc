using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using dbsc.Core;
using dbsc.Core.Sql;

namespace dbsc.Oracle
{
    class OraDbscEngine : SqlDbscEngine<DbConnectionInfo, SqlCheckoutSettings, ImportSettingsWithTableList<DbConnectionInfo>, SqlUpdateSettings, OraDbscDbConnection>
    {
        protected override char QueryParamChar { get { return ':'; } }

        protected override bool CheckoutAndUpdateIsSupported(out string whyNot)
        {
            if (!Utils.ExecutableIsOnPath("sqlplus", "-V"))
            {
                whyNot = "You must have the sqlplus command line program installed and on your PATH to check out and update databases.";
                return false;
            }
            else
            {
                whyNot = null;
                return true;
            }
        }

        protected override bool ImportIsSupported(out string whyNot)
        {
            throw new NotImplementedException("Importing not implemented for Oracle.");
        }
        
        protected override OraDbscDbConnection OpenConnection(DbConnectionInfo connectionInfo)
        {
            return new OraDbscDbConnection(connectionInfo);
        }

        protected override bool MetadataTableExists(OraDbscDbConnection conn)
        {
            string sql = "SELECT count(*) AS c FROM USER_TABLES WHERE TABLE_NAME = 'DBSC_METADATA'";
            return conn.Query<OracleCount>(sql).First().c > 0;
        }

        private class OracleCount
        {
            public int c { get; set; }
        }

        protected override DbConnectionInfo GetSystemDatabaseConnectionInfo(DbConnectionInfo database)
        {
            // The database actually already exists. We're not "creating" it, merely initializing it with the
            // creation template ad the dbsc metadata table
            return database.Clone();
        }

        protected override void CreateDatabase(SqlCheckoutSettings options)
        {
            // Target "database" already exists. What we're creating is the schema under a particular user.
            // Oracle is weird.
        }

        protected override string CreateMetadataTableSql
        {
            get
            {
                return
@"CREATE TABLE dbsc_metadata
(
    property_name nvarchar2(128) NOT NULL PRIMARY KEY,
    property_value nvarchar2(1000)
)";
            }
        }

        protected override void CreateMetadataProperties(OraDbscDbConnection conn, IDictionary<string, string> properties)
        {
            if (properties.Count == 0)
                return;

            // Oracle does not support the
            //
            // INSERT INTO dbsc_metadata
            // (property_name, property_value)
            // VALUES
            // (name0, value0)
            // ,(name1, value1)
            // ,(name2, value2)
            //
            // syntax that MySQL, PostgreSQL, and SQL Server do.

            //INSERT ALL
            //INTO dbsc_metadata (property_name, property_value) VALUES ('name0', 'value0')
            //INTO dbsc_metadata (property_name, property_value) VALUES ('name1', 'name1')
            //INTO dbsc_metadata (property_name, property_value) VALUES ('name2', 'name2')
            //SELECT * FROM DUAL

            StringBuilder sqlBuilder = new StringBuilder(string.Format(@"INSERT ALL
", MetadataTableName));
            int propertyNum = 0;
            Dictionary<string, object> sqlParams = new Dictionary<string, object>();
            foreach (string propertyName in properties.Keys)
            {
                sqlBuilder.AppendFormat("INTO {0} ({1}, {2}) VALUES ({3}name{4}, {3}value{4})\n",
                    MetadataTableName, MetadataPropertyNameColumn, MetadataPropertyValueColumn, QueryParamChar, propertyNum);
                sqlParams["name" + propertyNum.ToString(CultureInfo.InvariantCulture)] = propertyName;
                sqlParams["value" + propertyNum.ToString(CultureInfo.InvariantCulture)] = properties[propertyName];
                propertyNum++;
            }
            sqlBuilder.Append("SELECT * FROM DUAL"); // Really Oracle, why is a SELECT necessary after inserting multiple rows?

            string sql = sqlBuilder.ToString();
            conn.ExecuteSql(sql, sqlParams);
        }

        protected override void RunScriptAndUpdateMetadata(SqlUpdateSettings options, string scriptPath, int newRevision)
        {
            using (OraDbscDbConnection conn = OpenConnection(options.TargetDatabase))
            {
                conn.ExecuteSqlScriptFromFile(scriptPath);

                string newRevisionString = newRevision.ToString(CultureInfo.InvariantCulture);
                UpdateMetadataProperty(conn, RevisionPropertyName, newRevisionString);

                string utcTimestampString = GetCurrentTimestampString();
                UpdateMetadataProperty(conn, LastUpdatedPropertyName, utcTimestampString);
            }
        }

        public override void ImportData(SqlUpdateSettings options, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadataAlreadyEscaped)
        {
            throw new NotImplementedException("Importing not implemented for Oracle.");
        }

        public override ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(DbConnectionInfo connectionInfo)
        {
            throw new NotImplementedException("Importing not implemented for Oracle.");
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
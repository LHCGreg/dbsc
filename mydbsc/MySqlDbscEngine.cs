﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using MySql.Data.MySqlClient;
using Dapper;
using dbsc.Core;
using dbsc.Core.Sql;
using dbsc.Core.ImportTableSpecification;

namespace dbsc.MySql
{
    class MySqlDbscEngine : SqlDbscEngine<DbConnectionInfo, MySqlCheckoutSettings, MySqlImportSettings, MySqlUpdateSettings, MySqlDbscDbConnection, TableAndRule<MySqlTable, TableWithoutSchemaSpecification>>
    {
        protected override char QueryParamChar { get { return '@'; } }
        
        protected override DbConnectionInfo GetSystemDatabaseConnectionInfo(DbConnectionInfo targetDatabase)
        {
            DbConnectionInfo noDatabase = targetDatabase.Clone();
            noDatabase.Database = null;
            return noDatabase;
        }

        protected override MySqlDbscDbConnection OpenConnection(DbConnectionInfo connectionInfo)
        {
            return new MySqlDbscDbConnection(connectionInfo);
        }

        protected override string CreateMetadataTableSql
        {
            get
            {
                return
@"CREATE TABLE dbsc_metadata
(
    property_name nvarchar(128) NOT NULL PRIMARY KEY,
    property_value text
)
ENGINE=InnoDB";
            }
        }

        protected override string MetadataTableName { get { return "dbsc_metadata"; } }
        protected override string MetadataPropertyNameColumn { get { return "property_name"; } }
        protected override string MetadataPropertyValueColumn { get { return "property_value"; } }

        private class Table
        {
            public string TABLE_NAME { get; set; }
        }

        protected override bool MetadataTableExists(MySqlDbscDbConnection conn)
        {
            string sql =
@"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'dbsc_metadata'
AND TABLE_SCHEMA = @db";

            Dictionary<string, object> sqlParams = new Dictionary<string, object>() { { "db", conn.ConnectionInfo.Database } };

            Table metadataTable = conn.Query<Table>(sql, sqlParams).FirstOrDefault();
            return metadataTable != null;
        }

        protected override bool CheckoutAndUpdateIsSupported(out string whyNot)
        {
            whyNot = null;
            return true;
        }

        protected override bool ImportIsSupported(out string whyNot)
        {
            // mysql and mysqldump must be on the PATH.
            if (!Utils.ExecutableIsOnPath("mysqldump", "--version"))
            {
                whyNot = "Importing is not supported because you do not have mysqldump installed and on your PATH.";
                return false;
            }

            if (!Utils.ExecutableIsOnPath("mysql", "--version"))
            {
                whyNot = "Importing is not supported because you do not have mysql installed and on your PATH.";
                return false;
            }

            whyNot = null;
            return true;
        }

        public override ICollection<TableAndRule<MySqlTable, TableWithoutSchemaSpecification>> GetTablesToImport(MySqlUpdateSettings updateSettings)
        {
            MySqlImportTableCalculator tableCalculator = new MySqlImportTableCalculator();
            using (MySqlDbscDbConnection conn = OpenConnection(updateSettings.TargetDatabase))
            {
                return tableCalculator.GetTablesToImport(conn, updateSettings.ImportOptions.TablesToImportSpecifications);
            }
        }

        public override void ImportData(MySqlUpdateSettings updateSettings, ICollection<TableAndRule<MySqlTable, TableWithoutSchemaSpecification>> tablesToImport)
        {
            // MS SQL Server and PostgreSQL have simple methods of streaming bulk data to the DB server. MySQL does not.
            // The best MySQL can do is accept a file with the bulk data in it.
            // Correctly formatting that data could be tricky (what to do for dates, for example).
            // The easy but less efficient way is to simply do a mysqldump from the source DB and run it on the target DB.
            // MySQL is dumb, let's go shopping!

            const int enableConstraintsTimeoutInSeconds = 60 * 60 * 6;

            using (MySqlDbscDbConnection targetConn = OpenConnection(updateSettings.TargetDatabase))
            {
                // Disable foreign key constraints
                string disableForeignKeyChecksSql = "SET foreign_key_checks = 0";
                Console.WriteLine(disableForeignKeyChecksSql);
                targetConn.ExecuteSql(disableForeignKeyChecksSql);

                // Disable unique checks for performance
                string disableUniqueChecksSql = "SET unique_checks = 0";
                Console.WriteLine(disableUniqueChecksSql);
                targetConn.ExecuteSql(disableUniqueChecksSql);

                // Can only disable indexes on MyISAM tables, so don't do that for now.

                string clearMessage = "Clearing tables to import";

                // Clear tables
                Utils.DoTimedOperation(clearMessage, () =>
                {
                    foreach (MySqlTable table in tablesToImport.Select(t => t.Table))
                    {
                        string clearTableSql = string.Format("TRUNCATE TABLE {0}", table);
                        targetConn.ExecuteSql(clearTableSql);
                    }
                });

                // Import each table
                foreach (MySqlTable table in tablesToImport.Select(t => t.Table))
                {
                    string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sql");
                    Utils.DoTimedOperationThatOuputsStuff(string.Format("Making mysqldump of {0}", table), () =>
                    {
                        string mysqldumpArgs = string.Format("--no-defaults --skip-comments --skip-add-drop-table --no-create-info --no-autocommit {0} {1} {2} {3} {4} {5} {6}",
                            string.Format("--host={0}", updateSettings.ImportOptions.SourceDatabase.Server).QuoteCommandLineArg(),
                            updateSettings.ImportOptions.SourceDatabase.Port != null ? string.Format(CultureInfo.InvariantCulture, "--port={0}", updateSettings.ImportOptions.SourceDatabase.Port.Value).QuoteCommandLineArg() : "",
                            string.Format("--user={0}", updateSettings.ImportOptions.SourceDatabase.Username).QuoteCommandLineArg(),
                            string.Format("--password={0}", updateSettings.ImportOptions.SourceDatabase.Password).QuoteCommandLineArg(),
                            string.Format("--result-file={0}", tempFilePath).QuoteCommandLineArg(),
                            updateSettings.ImportOptions.SourceDatabase.Database.QuoteCommandLineArg(),
                            table.Table.QuoteCommandLineArg()
                            );

                        Process mysqldump = new Process()
                        {
                            StartInfo = new ProcessStartInfo("mysqldump", mysqldumpArgs)
                            {
                                CreateNoWindow = true,
                                ErrorDialog = false,
                                RedirectStandardError = true,
                                // Don't redirect stdout. There shouldn't be any
                                //RedirectStandardOutput = true,
                                UseShellExecute = false,
                            },
                            EnableRaisingEvents = true
                        };

                        mysqldump.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
                        using (mysqldump)
                        {
                            mysqldump.Start();
                            mysqldump.BeginErrorReadLine();
                            mysqldump.WaitForExit();
                            if (mysqldump.ExitCode != 0)
                            {
                                throw new DbscException("mysqldump error.");
                            }
                        }
                    });

                    try
                    {
                        Utils.DoTimedOperationThatOuputsStuff(string.Format("Importing mysqldump of {0}", table), () =>
                        {
                            string mysqlArgs = string.Format("{0} {1} {2} {3} {4}",
                                string.Format("--database={0}", updateSettings.TargetDatabase.Database).QuoteCommandLineArg(),
                                string.Format("--host={0}", updateSettings.TargetDatabase.Server).QuoteCommandLineArg(),
                                updateSettings.TargetDatabase.Port != null ? string.Format(CultureInfo.InvariantCulture, "--port={0}", updateSettings.TargetDatabase.Port.Value).QuoteCommandLineArg() : "",
                                string.Format("--user={0}", updateSettings.TargetDatabase.Username).QuoteCommandLineArg(),
                                string.Format("--password={0}", updateSettings.TargetDatabase.Password).QuoteCommandLineArg()
                            );

                            Process mysql = new Process()
                            {
                                StartInfo = new ProcessStartInfo("mysql", mysqlArgs)
                                {
                                    CreateNoWindow = true,
                                    ErrorDialog = false,
                                    RedirectStandardError = true,
                                    RedirectStandardInput = true,
                                    UseShellExecute = false
                                }
                            };

                            mysql.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
                            using (FileStream dumpFile = File.OpenRead(tempFilePath))
                            using (mysql)
                            {
                                mysql.Start();
                                mysql.BeginErrorReadLine();
                                dumpFile.CopyTo(mysql.StandardInput.BaseStream);
                                mysql.StandardInput.BaseStream.Flush();
                                mysql.StandardInput.Close();
                                mysql.WaitForExit();
                                if (mysql.ExitCode != 0)
                                {
                                    throw new DbscException("mysql error.");
                                }
                            }
                        });
                    }
                    finally
                    {
                        try
                        {
                            File.Delete(tempFilePath);
                        }
                        catch
                        {
                            ; // Not much we can do if we can't delete the temp file for some reason.
                        }
                    }
                }

                // Enable unique checks
                string enableUniqueChecksSql = "SET unique_checks = 1";
                Console.WriteLine(enableUniqueChecksSql);
                targetConn.ExecuteSql(enableUniqueChecksSql, timeoutInSeconds: enableConstraintsTimeoutInSeconds);

                // Enable foreign key constraints
                string enableForeignKeyChecksSql = "SET foreign_key_checks = 1";
                Console.WriteLine(enableForeignKeyChecksSql);
                targetConn.ExecuteSql(enableForeignKeyChecksSql, timeoutInSeconds: enableConstraintsTimeoutInSeconds);
            }
        }
    }
}

// Copyright (C) 2014 Greg Najda
//
// This file is part of mydbsc.
//
// mydbsc is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// mydbsc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with mydbsc.  If not, see <http://www.gnu.org/licenses/>.
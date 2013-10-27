﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Dapper;
using dbsc.Core;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace dbsc.MySql
{
    class MySqlDbscEngine : SqlDbscEngine<SqlCheckoutOptions, SqlUpdateOptions, MySqlDbscDbConnection>
    {
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
)";
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
            DbConnectionInfo connInfo = conn.ConnectionInfo;
            string sql =
@"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'dbsc_metadata'
AND TABLE_SCHEMA = @db";

            Dictionary<string, object> sqlParams = new Dictionary<string, object>() { { "db", connInfo.Database } };

            Table metadataTable = conn.Query<Table>(sql, sqlParams).FirstOrDefault();
            return metadataTable != null;
        }

        protected override ICollection<string> GetTableNamesExceptMetadataAlreadyEscaped(MySqlDbscDbConnection conn)
        {
            DbConnectionInfo connInfo = conn.ConnectionInfo;
            string sql =
@"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME <> 'dbsc_metadata'
AND TABLE_SCHEMA = @db
AND TABLE_TYPE = 'BASE TABLE'";

            Dictionary<string, object> sqlParams = new Dictionary<string, object>() { { "db", connInfo.Database } };

            List<string> tables = conn.Query<Table>(sql, sqlParams).Select(table => table.TABLE_NAME).ToList();
            return tables;
        }

        protected override bool ImportIsSupported(out string whyNot)
        {
            // mysql and mysqldump must be on the PATH.
            using (Process mysqldump = new Process()
            {
                StartInfo = new ProcessStartInfo("mysqldump", "--version")
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    UseShellExecute = false
                },
            })
            {
                try
                {
                    mysqldump.Start();
                }
                catch (Exception)
                {
                    whyNot = "Importing is not supported because you do not have mysqldump installed and on your PATH.";
                    return false;
                }
            }

            using (Process mysql = new Process()
            {
                StartInfo = new ProcessStartInfo("mysql", "--version")
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    UseShellExecute = false
                },
            })
            {
                try
                {
                    mysql.Start();
                }
                catch (Exception)
                {
                    whyNot = "Importing is not supported because you do not have mysql installed and on your PATH.";
                    return false;
                }
            }

            whyNot = null;
            return true;
        }

        protected override void ImportData(SqlUpdateOptions options, ICollection<string> tablesToImportAlreadyEscaped, ICollection<string> allTablesExceptMetadataAlreadyEscaped)
        {
            // MS SQL Server and PostgreSQL have simple methods of streaming bulk data to the DB server. MySQL does not.
            // The best MySQL can do is accept a file with the bulk data in it.
            // Correctly formatting that data could be tricky (what to do for dates, for example).
            // The easy but less efficient way is to simply do a mysqldump from the source DB and run it on the target DB.
            // MySQL is dumb, let's go shopping!

            const int enableConstraintsTimeoutInSeconds = 60 * 60 * 6;

            using (MySqlDbscDbConnection targetConn = OpenConnection(options.TargetDatabase))
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

                // Clear tables
                ImportUtils.DoTimedOperation("Clearing all tables", () =>
                {
                    foreach (string table in allTablesExceptMetadataAlreadyEscaped)
                    {
                        string clearTableSql = string.Format("TRUNCATE TABLE {0}", table);
                        targetConn.ExecuteSql(clearTableSql);
                    }
                });

                // Import each table
                foreach (string table in tablesToImportAlreadyEscaped)
                {
                    string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sql");
                    ImportUtils.DoTimedOperationThatOuputsStuff(string.Format("Making mysqldump of {0}", table), () =>
                    {
                        string mysqldumpArgs = string.Format("--no-defaults --skip-comments --skip-add-drop-table --no-create-info --no-autocommit {0} {1} {2} {3} {4} {5} {6}",
                            string.Format("--host={0}", options.ImportOptions.SourceDatabase.Server).QuoteCommandLineArg(),
                            options.ImportOptions.SourceDatabase.Port != null ? string.Format(CultureInfo.InvariantCulture, "--port={0}", options.ImportOptions.SourceDatabase.Port.Value).QuoteCommandLineArg() : "",
                            string.Format("--user={0}", options.ImportOptions.SourceDatabase.Username).QuoteCommandLineArg(),
                            string.Format("--password={0}", options.ImportOptions.SourceDatabase.Password).QuoteCommandLineArg(),
                            string.Format("--result-file={0}", tempFilePath).QuoteCommandLineArg(),
                            options.ImportOptions.SourceDatabase.Database.QuoteCommandLineArg(),
                            table.QuoteCommandLineArg()
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

                        mysqldump.ErrorDataReceived += (sender, e) => Console.Write(e.Data);
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
                        ImportUtils.DoTimedOperationThatOuputsStuff(string.Format("Importing mysqldump of {0}", table), () =>
                        {
                            string mysqlArgs = string.Format("{0} {1} {2} {3} {4}",
                                string.Format("--database={0}", options.TargetDatabase.Database).QuoteCommandLineArg(),
                                string.Format("--host={0}", options.TargetDatabase.Server).QuoteCommandLineArg(),
                                options.TargetDatabase.Port != null ? string.Format(CultureInfo.InvariantCulture, "--port={0}", options.TargetDatabase.Port.Value).QuoteCommandLineArg() : "",
                                string.Format("--user={0}", options.TargetDatabase.Username).QuoteCommandLineArg(),
                                string.Format("--password={0}", options.TargetDatabase.Password).QuoteCommandLineArg()
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

// Copyright (C) 2013 Greg Najda
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
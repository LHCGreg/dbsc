using dbsc.Core;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace dbsc.Mongo
{
    class MongoDbscConnection : IDisposable
    {
        private MongoServer m_server;
        private MongoDatabase m_database;
        private DbConnectionInfo m_connectionInfo;

        public MongoDbscConnection(DbConnectionInfo connectionInfo)
        {
            m_connectionInfo = connectionInfo;

            MongoUrl url = GetMongoUrl(m_connectionInfo);

            MongoClient client = new MongoClient(url);
            m_server = client.GetServer();
            m_server.Connect();
            m_database = m_server.GetDatabase(connectionInfo.Database);
        }

        private static MongoUrl GetMongoUrl(DbConnectionInfo connectionInfo)
        {
            MongoUrlBuilder urlBuilder = new MongoUrlBuilder();
            urlBuilder.ConnectTimeout = TimeSpan.FromSeconds(connectionInfo.ConnectTimeoutInSeconds);
            urlBuilder.DatabaseName = connectionInfo.Database;
            urlBuilder.Password = connectionInfo.Password;
            urlBuilder.Username = connectionInfo.Username;

            if (connectionInfo.Port == null)
            {
                urlBuilder.Server = new MongoServerAddress(connectionInfo.Server);
            }
            else
            {
                urlBuilder.Server = new MongoServerAddress(connectionInfo.Server, connectionInfo.Port.Value);
            }

            urlBuilder.SocketTimeout = TimeSpan.FromSeconds(connectionInfo.CommandTimeoutInSeconds);

            return urlBuilder.ToMongoUrl();
        }

        public bool DatabaseExists(string databaseName)
        {
            return m_server.DatabaseExists(databaseName);
        }

        public void CreateDatabase(string databaseName)
        {
            // Mongo creates databases when you insert something into one.
            // You cannot simply tell it "create a database named foo".

            // Make sure the database doesn't already exist
            if (m_server.DatabaseExists(databaseName))
            {
                throw new DbscException(string.Format(
                        "Database {0} already exists on {1}.",
                        databaseName, m_connectionInfo.Server));
            }

            MongoDatabase newDb = m_server.GetDatabase(databaseName);
            // if authenticating, add self as an admin user
            if (m_connectionInfo.Username != null && m_connectionInfo.Password != null)
            {
                newDb.AddUser(new MongoUser(m_connectionInfo.Username, new PasswordEvidence(m_connectionInfo.Password), isReadOnly: false));
            }

            // create a temporary collection and drop it so that the database gets created.
            newDb.CreateCollection("dbsc_temp");
            newDb.DropCollection("dbsc_temp");
        }

        public bool ContainsCollection(string collectionName)
        {
            return m_database.CollectionExists(collectionName);
        }

        public void CreateCollection(string collectionName)
        {
            // Don't use a capped collection for the metadata collection because you cannot do updates
            // on documents in a capped collection that would cause the document to grow in size.
            m_database.CreateCollection(collectionName);
        }

        public void DropCollection(string collectionName)
        {
            m_database.DropCollection(collectionName);
            CommandDocument cloneCommand = new CommandDocument();
            cloneCommand["cloneCollection"] = "";
            cloneCommand["from"] = "";
        }

        public void Upsert<T>(T document, string collectionName)
        {
            MongoCollection<T> collection = m_database.GetCollection<T>(collectionName);
            collection.Save(document, new MongoInsertOptions() { WriteConcern = new WriteConcern() { WTimeout = TimeSpan.FromSeconds(m_connectionInfo.CommandTimeoutInSeconds) } });
        }

        public void UpdateAll<T>(string collectionName, IEnumerable<Tuple<Expression<Func<T, object>>, object>> updates)
        {
            QueryDocument query = new QueryDocument();

            UpdateBuilder<T> updateBuilder = new UpdateBuilder<T>();
            foreach (Tuple<Expression<Func<T, object>>, object> updateLambdaAndObject in updates)
            {
                updateBuilder.Set(updateLambdaAndObject.Item1, updateLambdaAndObject.Item2);
            }

            MongoDB.Driver.MongoUpdateOptions updateOptions = new MongoDB.Driver.MongoUpdateOptions()
            {
                Flags = UpdateFlags.Multi,
                WriteConcern = new WriteConcern() { WTimeout = TimeSpan.FromSeconds(m_connectionInfo.CommandTimeoutInSeconds) }
            };

            MongoCollection collection = m_database.GetCollection(collectionName);
            collection.Update(query, updateBuilder, updateOptions);
        }

        public T GetSingleDocument<T>(string collectionName)
        {
            MongoCollection<T> collection = m_database.GetCollection<T>(collectionName);
            return collection.FindOne();
        }

        public ICollection<string> GetCollectionNames()
        {
            return m_database.GetCollectionNames().Where(collName => !collName.StartsWith("system.")).ToList();
        }

        private List<string> GetCommonMongoDumpRestoreArgs(DbConnectionInfo source, string collectionName)
        {
            List<string> args = new List<string>();
            args.Add(string.Format("--host {0}", source.Server.QuoteCommandLineArg()));

            if (source.Port != null)
            {
                args.Add(string.Format("--port {0}", source.Port.Value.ToString(CultureInfo.InvariantCulture).QuoteCommandLineArg()));
            }

            if (source.Username != null)
            {
                args.Add(string.Format("-u {0}", source.Username.QuoteCommandLineArg()));
            }

            if (source.Password != null)
            {
                args.Add(string.Format("-p {0}", source.Password.QuoteCommandLineArg()));
            }

            args.Add(string.Format("-db {0}", source.Database.QuoteCommandLineArg()));
            args.Add(string.Format("-c {0}", collectionName.QuoteCommandLineArg()));

            return args;
        }

        public void ImportCollection(DbConnectionInfo source, DbConnectionInfo target, string collectionName)
        {
            // Mongo has a cloneCollection command but it has some serious limitations:
            // - Can only clone from a remote server, not the same server
            // - Cannot clone from servers that require authentication
            // - Separate commands for regular collections vs. capped collections

            // Mongo has a copydb command but...
            // - We don't really want to copy the entire database, only the collections that we have.
            //   The source could have additional temp collections or such
            // - This prevents specifying only certain collections to clone
            // - The docs mention some strange hoops you have to go through to authenticate

            // So shelling out to mongodump/mongorestore it is!
            // Might be worth using cloneCollection in cases where the limitations don't apply for a speed boost

            // mongodump --host server [--port port] [-u username -p password] -db sourcedb -c collection -o outputDir

            List<string> mongodumpArgs = GetCommonMongoDumpRestoreArgs(source, collectionName);

            string tempDirPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");
            mongodumpArgs.Add(string.Format("-o {0}", tempDirPath.QuoteCommandLineArg()));

            string mongodumpArgsString = string.Join(" ", mongodumpArgs);

            Utils.DoTimedOperationThatOuputsStuff(string.Format("Doing mongodump of {0} on {1}", collectionName, source.Database), () =>
            {
                Process mongodump = new Process()
                {
                    StartInfo = new ProcessStartInfo("mongodump", mongodumpArgsString)
                    {
                        CreateNoWindow = true,
                        ErrorDialog = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                    },
                    EnableRaisingEvents = true
                };

                object consoleLock = new object();

                mongodump.OutputDataReceived += (sender, e) => { lock (consoleLock) { Console.WriteLine(e.Data); } };
                mongodump.ErrorDataReceived += (sender, e) => { lock (consoleLock) { Console.WriteLine(e.Data); } };
                using (mongodump)
                {
                    mongodump.Start();
                    mongodump.BeginOutputReadLine();
                    mongodump.BeginErrorReadLine();
                    mongodump.WaitForExit();
                    if (mongodump.ExitCode != 0)
                    {
                        throw new DbscException("mongodump error. Check the error above for details.");
                    }
                }
            });

            try
            {
                // mongorestore --host server [--port port] [-u username -p password] -db targetDb -c collection outputDir
                List<string> mongorestoreArgs = GetCommonMongoDumpRestoreArgs(target, collectionName);

                // The path that we need to pass to mongorestore is actually tempDirPath\db_name\collection_name.bson
                // Let's not try to guess how mongo escapes characters that are not valid file system characters
                string dbFolder = Directory.EnumerateDirectories(tempDirPath).FirstOrDefault();
                if (dbFolder == null)
                {
                    throw new DbscException("mongodump did not create a directory.");
                }

                string bsonFilePath = Directory.EnumerateFiles(dbFolder, "*.bson").FirstOrDefault();
                if (bsonFilePath == null)
                {
                    throw new DbscException("mongodump did not create a .bson file.");
                }

                mongorestoreArgs.Add(bsonFilePath.QuoteCommandLineArg());

                string mongorestoreArgsString = string.Join(" ", mongorestoreArgs);

                Utils.DoTimedOperationThatOuputsStuff(string.Format("Doing mongorestore of {0} on {1}", collectionName, target.Database), () =>
                {
                    Process mongorestore = new Process()
                    {
                        StartInfo = new ProcessStartInfo("mongorestore", mongorestoreArgsString)
                        {
                            CreateNoWindow = true,
                            ErrorDialog = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                        },
                        EnableRaisingEvents = true
                    };

                    object consoleLock = new object();

                    mongorestore.OutputDataReceived += (sender, e) => { lock (consoleLock) { Console.WriteLine(e.Data); } };
                    mongorestore.ErrorDataReceived += (sender, e) => { lock (consoleLock) { Console.WriteLine(e.Data); } };
                    using (mongorestore)
                    {
                        mongorestore.Start();
                        mongorestore.BeginOutputReadLine();
                        mongorestore.BeginErrorReadLine();
                        mongorestore.WaitForExit();
                        if (mongorestore.ExitCode != 0)
                        {
                            throw new DbscException("mongorestore error. Check the output above for details.");
                        }
                    }
                });
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDirPath, recursive: true);
                }
                catch
                {
                    ; // If cleaning up the temp directory fails, whatever, it's in the temp directory.
                }
            }
        }

        public void Dispose()
        {
            ; // The Mongo driver does its own management of connections and its classes do not implement IDisposable.
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
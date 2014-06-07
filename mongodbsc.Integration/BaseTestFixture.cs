using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TestUtils;

namespace dbsc.Mongo.Integration
{
    [TestFixture]
    abstract class BaseTestFixture
    {
        protected static readonly string TestDatabaseName = "mongodbsc_test";
        protected static readonly string AltTestDatabaseName = "mongodbsc_test_2";
        protected static readonly string SourceDatabaseName = "mongodbsc_test_source";
        protected static readonly string AltSourceDatabaseName = "mongodbsc_test_source_2";

        protected string MongodbscPath { get; private set; }
        protected string ScriptsDir { get; private set; }
        protected string ScriptsForOtherDBDir { get { return Path.Combine(ScriptsDir, "..", "scripts_for_other_db"); } }

        [TestFixtureSetUp]
        public void SetDirectories()
        {
            Uri thisAssemblyUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string thisAssemblyPath = thisAssemblyUri.LocalPath;
            string thisAssemblyDir = Path.GetDirectoryName(thisAssemblyPath);
            MongodbscPath = Path.Combine(thisAssemblyDir, "mongodbsc.exe");
            ScriptsDir = Path.Combine(thisAssemblyDir, "scripts");
        }

        protected List<Book> ExpectedBooks = new List<Book>()
        {
            new Book()
            {
                name = "A Game of Thrones",
                author = "George R.R. Martin"
            },
            new Book()
            {
                name = "Clean Code",
			    author = "Robert C. Martin"
            },
            new Book()
            {
                name = "The Mythical Man-Month",
			    author = "Frederick P. Brooks, Jr."
            }
        };

        protected List<Book> ExpectedSourceBooks = new List<Book>()
        {
            new Book()
            {
                name = "Charlie and the Chocolate Factory",
                author = "Roald Dahl"
            }
        };

        protected List<Book> BooksFromCreationTemplate = new List<Book>()
        {
            new Book()
            {
                name = "The Eye of the World",
                author = "Robert Jordan"
            }
        };

        protected List<Book> ExpectedAltSourceBooks = new List<Book>()
        {
            new Book()
            {
                name = "Charlie and the Chocolate Factory",
                author = "Roald Dahl"
            },
            new Book()
            {
                name = "A Feast for Crows",
                author = "George R.R. Martin"
            }
        };

        protected List<Person> ExpectedPeople = new List<Person>()
        {
            new Person()
            {
                name = "Greg",
                preferences = new PersonPreferences()
                {
                    a = 500,
                    b = new List<int>() { 800, 900 },
                    c = true
                }
            },
            new Person()
            {
                name = "Joe",
                preferences = new PersonPreferences()
                {
                    a = 1000,
                    b = null,
                    c = false
                }
            }
        };

        protected List<Number> ExpectedNumbers = new List<Number>()
        {
            new Number()
            {
                num = 1,
                english = "one",
                spanish = "uno"
            },
            new Number()
            {
                num = 2,
                english = "two",
                spanish = "dos"
            }
        };

        protected void DropDatabase(string dbName)
        {
            MongoClient mongoClient = new MongoClient("mongodb://localhost");
            MongoServer server = mongoClient.GetServer();
            if (server.DatabaseExists(dbName))
            {
                server.DropDatabase(dbName);
            }
        }

        private string GetAuthServerConnectionString()
        {
            MongoConnectionStringBuilder builder = new MongoConnectionStringBuilder();
            builder.Server = new MongoServerAddress("localhost", 30017);
            builder.Username = "useradmin";
            builder.Password = "testpw";
            return builder.ToString();
        }

        protected void DropDatabaseOnAuthMongo(string dbName)
        {
            string connectionString = GetAuthServerConnectionString();
            MongoClient mongoClient = new MongoClient(connectionString);
            MongoServer server = mongoClient.GetServer();
            if (server.DatabaseExists(dbName))
            {
                server.DropDatabase(dbName);
            }
        }

        protected void RunSuccessfulCommand(string arguments)
        {
            ProcessUtils.RunSuccessfulCommand(MongodbscPath, arguments, ScriptsDir);
        }

        protected void RunSuccessfulCommand(string arguments, out string stdout, out string stderr)
        {
            ProcessUtils.RunSuccessfulCommand(MongodbscPath, arguments, ScriptsDir, out stdout, out stderr);
        }

        protected void RunUnsuccessfulCommand(string arguments)
        {
            ProcessUtils.RunUnsuccessfulCommand(MongodbscPath, arguments, ScriptsDir);
        }

        protected void RunUnsuccessfulCommand(string arguments, out string stdout, out string stderr)
        {
            ProcessUtils.RunUnsuccessfulCommand(MongodbscPath, arguments, ScriptsDir, out stdout, out stderr);
        }

        protected void VerifyDatabaseOnAuthMongo(string databaseName, List<Book> expectedBooks, List<Person> expectedPeople, List<Number> expectedNumbers, int expectedVersion)
        {
            string connectionString = GetAuthServerConnectionString();
            VerifyDatabase(connectionString, databaseName, expectedBooks, expectedPeople, expectedNumbers, expectedVersion);
        }

        protected void VerifyDatabase(string databaseName, List<Book> expectedBooks, List<Person> expectedPeople, List<Number> expectedNumbers, int expectedVersion)
        {
            VerifyDatabase("mongodb://localhost", databaseName, expectedBooks, expectedPeople, expectedNumbers, expectedVersion);
        }

        protected void VerifyDatabase(string connectionString, string databaseName, List<Book> expectedBooks, List<Person> expectedPeople, List<Number> expectedNumbers, int expectedVersion)
        {
            MongoClient mongoClient = new MongoClient(connectionString);
            MongoServer server = mongoClient.GetServer();
            MongoDatabase database = server.GetDatabase(databaseName);

            MongoCollection<Book> bookCollection = database.GetCollection<Book>("books");
            List<Book> books = bookCollection.FindAll().ToList();

            Assert.That(books, Is.EquivalentTo(expectedBooks));

            GetIndexesResult indexes = bookCollection.GetIndexes();
            BsonDocument expectedIndexKey = new BsonDocument();
            expectedIndexKey["name"] = 1;
            Assert.That(indexes.Any(index => index.Key == expectedIndexKey));

            MongoCollection<Person> personCollection = database.GetCollection<Person>("people");
            List<Person> people = personCollection.FindAll().ToList();

            Assert.That(people, Is.EquivalentTo(expectedPeople));

            MongoCollection<Number> numberCollection = database.GetCollection<Number>("numbers");
            List<Number> numbers = numberCollection.FindAll().ToList();

            Assert.That(numbers, Is.EquivalentTo(expectedNumbers));

            MongoCollection<dbsc_metadata> metadataCollection = database.GetCollection<dbsc_metadata>("dbsc_metadata");
            List<dbsc_metadata> metadataList = metadataCollection.FindAll().ToList();
            Assert.That(metadataList.Count, Is.EqualTo(1));
            dbsc_metadata metadata = metadataList[0];
            Assert.That(metadata.Version, Is.EqualTo(expectedVersion));
            Assert.That(metadata.MasterDatabaseName, Is.EqualTo("mongodbsc_test"));
            Assert.That(metadata.LastChangeUTC, Is.LessThan(DateTime.UtcNow + TimeSpan.FromMinutes(5)));
            Assert.That(metadata.LastChangeUTC, Is.GreaterThan(DateTime.UtcNow - TimeSpan.FromMinutes(5)));
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
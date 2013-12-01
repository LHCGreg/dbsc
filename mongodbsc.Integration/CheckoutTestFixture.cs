using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using MongoDB.Driver;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using MongoDB.Bson;

namespace dbsc.Mongo.Integration
{
    [TestFixture]
    public class CheckoutTestFixture
    {
        private static readonly string TestDatabaseName = "mongodbsc_test";
        private static readonly string AltTestDatabaseName = "mongodbsc_test_2";
        private static readonly string SourceDatabaseName = "mongodbsc_test_source";
        private static readonly string CreationTemplateDatabaseName = "creation_template_test";

        private string MongodbscPath { get; set; }
        private string ScriptsDir { get; set; }

        private List<Book> ExpectedBooks = new List<Book>()
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

        private List<Book> ExpectedSourceBooks = new List<Book>()
        {
            new Book()
            {
                name = "Charlie and the Chocolate Factory",
                author = "Roald Dahl"
            }
        };

        List<Person> ExpectedPeople = new List<Person>()
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

        List<Number> ExpectedNumbers = new List<Number>()
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

        [TestFixtureSetUp]
        public void SetDirectories()
        {
            Uri thisAssemblyUri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string thisAssemblyPath = thisAssemblyUri.LocalPath;
            string thisAssemblyDir = Path.GetDirectoryName(thisAssemblyPath);
            MongodbscPath = Path.Combine(thisAssemblyDir, "mongodbsc.exe");
            ScriptsDir = Path.Combine(thisAssemblyDir, "scripts");
        }

        [Test]
        public void BasicTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand("checkout");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void BasicImportTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand(string.Format("checkout -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void ImportOnlyOneCollectionTest()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand(string.Format("checkout -sourceDbServer localhost -sourceDb {0} -importTableList tables_to_import.txt", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, new List<Person>(), new List<Number>());
        }

        [Test]
        public void TestTargetDb()
        {
            DropDatabase(TestDatabaseName);
            DropDatabase(AltTestDatabaseName);

            // First get the source database into the the main test database
            RunSuccesfulCommand(string.Format("checkout -targetDb {0} -sourceDbServer localhost -sourceDb {1}", TestDatabaseName, SourceDatabaseName));

            // Then import from the main test database into the alt test database
            RunSuccesfulCommand(string.Format("checkout -targetDbServer localhost -targetDb {0} -port 27017 -sourceDbServer localhost -sourcePort 27017", AltTestDatabaseName));
            VerifyDatabase(AltTestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void TestNonexistantTargetDbServer()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -targetDbServer doesnotexist.local");
        }

        [Test]
        public void TestNonexistantTargetPort()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -port 9999");
        }

        [Test]
        public void TestNonExistantSourcePort()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -sourceDbServer localhost -sourcePort 9999");
        }

        [Test]
        public void TestCreationTemplate()
        {
            DropDatabase(TestDatabaseName);
            DropDatabase(CreationTemplateDatabaseName);
            RunSuccesfulCommand("checkout -dbCreateTemplate creation_template.js");
            VerifyCreationTemplateDatabase();
        }

        [Test]
        public void TestErrorInScriptAborts()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -dir ../error_test_scripts");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>());
        }

        [Test]
        public void TestPartialCheckout()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand("checkout -r 1");
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>());
        }

        [Test]
        public void TestPartialCheckoutWithImport()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand(string.Format("checkout -r 2 -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedSourceBooks, ExpectedPeople, ExpectedNumbers);
        }

        [Test]
        public void TestPartialCheckoutShortOfImport()
        {
            DropDatabase(TestDatabaseName);
            RunSuccesfulCommand(string.Format("checkout -r 1 -sourceDbServer localhost -sourceDb {0}", SourceDatabaseName));
            VerifyDatabase(TestDatabaseName, ExpectedBooks, ExpectedPeople, expectedNumbers: new List<Number>());
        }

        [Test]
        public void TestPartialCheckoutWithTooHighRevision()
        {
            DropDatabase(TestDatabaseName);
            RunUnsuccessfulCommand("checkout -r 3");
        }

        private void DropDatabase(string dbName)
        {
            MongoClient mongoClient = new MongoClient("mongodb://localhost");
            MongoServer server = mongoClient.GetServer();
            if (server.DatabaseExists(dbName))
            {
                server.DropDatabase(dbName);
            }
        }

        private int RunCommand(string arguments)
        {
            using (Process mongodbsc = new Process())
            {
                mongodbsc.StartInfo.Arguments = arguments;
                mongodbsc.StartInfo.FileName = MongodbscPath;
                mongodbsc.StartInfo.RedirectStandardOutput = true;
                mongodbsc.StartInfo.RedirectStandardError = true;
                mongodbsc.StartInfo.UseShellExecute = false;
                mongodbsc.StartInfo.WorkingDirectory = ScriptsDir;
                mongodbsc.StartInfo.CreateNoWindow = true;

                mongodbsc.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                mongodbsc.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                mongodbsc.Start();
                mongodbsc.BeginOutputReadLine();
                mongodbsc.BeginErrorReadLine();
                mongodbsc.WaitForExit();

                return mongodbsc.ExitCode;
            }
        }

        private void RunSuccesfulCommand(string arguments)
        {
            int returnCode = RunCommand(arguments);
            Assert.That(returnCode, Is.EqualTo(0));
        }

        private void RunUnsuccessfulCommand(string arguments)
        {
            int returnCode = RunCommand(arguments);
            Assert.That(returnCode, Is.Not.EqualTo(0));
        }

        private void VerifyDatabase(string databaseName, List<Book> expectedBooks, List<Person> expectedPeople, List<Number> expectedNumbers)
        {
            MongoClient mongoClient = new MongoClient("mongodb://localhost");
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
        }

        private void VerifyCreationTemplateDatabase()
        {
            MongoClient mongoClient = new MongoClient("mongodb://localhost");
            MongoServer server = mongoClient.GetServer();
            MongoDatabase database = server.GetDatabase(CreationTemplateDatabaseName);
            MongoCollection<TestCollectionObject> testCollection = database.GetCollection<TestCollectionObject>("testCollection");
            Assert.That(testCollection.FindAll().First().pass, Is.True);
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
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
    public class IntegrationTestFixture
    {
        private static readonly string TestDatabaseName = "mongodbsc_test";

        private string MongodbscPath { get; set; }
        private string ScriptsDir { get; set; }

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
            RunCommand("checkout");
            VerifyDatabase();
        }

        private void DropDatabase(string dbName)
        {
            MongoClient mongoClient = new MongoClient("mongodb://localhost");
            MongoServer server = mongoClient.GetServer();
            if (server.DatabaseExists(TestDatabaseName))
            {
                server.DropDatabase(TestDatabaseName);
            }
        }

        private void RunCommand(string arguments)
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
            }
        }

        private void VerifyDatabase()
        {
            MongoClient mongoClient = new MongoClient("mongodb://localhost");
            MongoServer server = mongoClient.GetServer();
            MongoDatabase database = server.GetDatabase(TestDatabaseName);

            MongoCollection<Book> bookCollection = database.GetCollection<Book>("books");
            List<Book> books = bookCollection.FindAll().ToList();

            List<Book> expectedBooks = new List<Book>()
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

            Assert.That(books, Is.EquivalentTo(expectedBooks));

            GetIndexesResult indexes = bookCollection.GetIndexes();
            BsonDocument expectedIndexKey = new BsonDocument();
            expectedIndexKey["name"] = 1;
            Assert.That(indexes.Any(index => index.Key == expectedIndexKey));

            MongoCollection<Person> personCollection = database.GetCollection<Person>("people");
            List<Person> people = personCollection.FindAll().ToList();

            List<Person> expectedPeople = new List<Person>()
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

            Assert.That(people, Is.EquivalentTo(expectedPeople));

            MongoCollection<Number> numberCollection = database.GetCollection<Number>("numbers");
            List<Number> numbers = numberCollection.FindAll().ToList();

            List<Number> expectedNumbers = new List<Number>()
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

            Assert.That(numbers, Is.EquivalentTo(expectedNumbers));
        }
    }
}

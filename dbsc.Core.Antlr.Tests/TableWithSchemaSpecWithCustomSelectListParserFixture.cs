using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using dbsc.Core.Antlr;
using dbsc.Core.ImportTableSpecification;
using dbsc.Core.Tests.ImportTableSpecification;

namespace dbsc.Core.Antlr.Tests
{
    [TestFixture]
    public class TableWithSchemaSpecWithCustomSelectListParserFixture
    {
        [Test]
        public void SqlServerBasicTest()
        {
            string inputString = @"
HelloWorld
[Hello World]

Hello . [World]
[Brackets]] are fun].[Really]]]]]]]]really fun]
[Let's have some * wildcards*].Regular*
[*].*
  ";

            BasicTest(inputString, Flavor.SqlServer);
        }

        [Test]
        public void PostgresBasicTest()
        {
            string inputString = @"
HelloWorld
""Hello World""

Hello . ""World""
""Brackets"""" are fun"".""Really""""""""""""""""really fun""
""Let's have some * wildcards*"".Regular*
""*"".*
  ";

            BasicTest(inputString, Flavor.Postgres);
        }

        private void BasicTest(string inputString, Flavor flavor)
        {
            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(null, Frag("HelloWorld"), negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(null, Frag("Hello World"), negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(Frag("Hello"), Frag("World"), negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(Frag("Brackets{0} are fun", flavor.ClosingQuoteChar), Frag("Really{0}{0}{0}{0}really fun", flavor.ClosingQuoteChar), negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(
                    new TableSpecificationFragment(
                        new List<StringOrWildcard>()
                        {
                            new StringOrWildcard("Let's have some "),
                            StringOrWildcard.Star,
                            new StringOrWildcard(" wildcards"),
                            StringOrWildcard.Star
                        }, caseSensitive: false),
                    new TableSpecificationFragment(
                        new List<StringOrWildcard>()
                        {
                            new StringOrWildcard("Regular"),
                            StringOrWildcard.Star
                        }, caseSensitive: false),
                    negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(
                    new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star }, caseSensitive: false),
                    new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star }, caseSensitive: false),
                    negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive)
            };

            Test(inputString, expectedSpecs, flavor);
        }

        [Test]
        public void MySqlBasicTest()
        {
            string inputString = @"
HelloWorld
`Hello World`

`Brackets`` are ""fun""`
""Quotes"""" are `too`""
`Let's have some * wildcards*`
""Let's have some * wildcards*""
Regular*
`*`
*
  ";

            Flavor mysql = Flavor.MySql;

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(null, Frag("HelloWorld"), negated: false, defaultSchemaIsCaseSensitive: mysql.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(null, Frag("Hello World"), negated: false, defaultSchemaIsCaseSensitive: mysql.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(null, Frag("Brackets` are \"fun\""), negated: false, defaultSchemaIsCaseSensitive: mysql.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(null, Frag("Quotes\" are `too`"), negated: false, defaultSchemaIsCaseSensitive: mysql.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(
                    null,
                    new TableSpecificationFragment(
                        new List<StringOrWildcard>()
                        {
                            new StringOrWildcard("Let's have some "),
                            StringOrWildcard.Star,
                            new StringOrWildcard(" wildcards"),
                            StringOrWildcard.Star
                        }, caseSensitive: false),
                    negated: false, defaultSchemaIsCaseSensitive: mysql.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(
                    null,
                    new TableSpecificationFragment(
                        new List<StringOrWildcard>()
                        {
                            new StringOrWildcard("Let's have some "),
                            StringOrWildcard.Star,
                            new StringOrWildcard(" wildcards"),
                            StringOrWildcard.Star
                        }, caseSensitive: false),
                    negated: false, defaultSchemaIsCaseSensitive: mysql.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(
                    null,
                    new TableSpecificationFragment(
                        new List<StringOrWildcard>()
                        {
                            new StringOrWildcard("Regular"),
                            StringOrWildcard.Star
                        }, caseSensitive: false),
                    negated: false, defaultSchemaIsCaseSensitive: mysql.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(
                    new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star }, caseSensitive: false),
                    new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star }, caseSensitive: false),
                    negated: false, defaultSchemaIsCaseSensitive: mysql.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(
                    new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star }, caseSensitive: false),
                    new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star }, caseSensitive: false),
                    negated: false, defaultSchemaIsCaseSensitive: mysql.DefaultSchemaCaseSensitive)
            };

            Test(inputString, expectedSpecs, Flavor.MySql);
        }

        [Test]
        public void MongoBasicTest()
        {
            Flavor flavor = Flavor.Mongo;
            
            string inputString = @"
HelloWorld

wild*cards*
*
";

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(null, Frag("HelloWorld"), negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(null, new TableSpecificationFragment(
                    new List<StringOrWildcard>()
                    {
                        new StringOrWildcard("wild"),
                        StringOrWildcard.Star,
                        new StringOrWildcard("cards"),
                        StringOrWildcard.Star
                    }
                    , caseSensitive: false)
                , negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(TableSpecificationFragment.Star, TableSpecificationFragment.Star, negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive)
            };

            Test(inputString, expectedSpecs, flavor);
        }

        [Test]
        public void SqlServerTestNonblankFirstLine()
        {
            TestNonblankFirstLine(Flavor.SqlServer);
        }

        [Test]
        public void PostgresTestNonblankFirstLine()
        {
            TestNonblankFirstLine(Flavor.Postgres);
        }

        [Test]
        public void MySqlTestNonblankFirstLine()
        {
            TestNonblankFirstLine(Flavor.MySql);
        }

        [Test]
        public void MongoTestNonblankFirstLine()
        {
            TestNonblankFirstLine(Flavor.Mongo);
        }

        private void TestNonblankFirstLine(Flavor flavor)
        {
            string inputString = "HelloWorld";

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(null, Frag("HelloWorld"), negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
            };

            Test(inputString, expectedSpecs, flavor);
        }

        [Test]
        public void SqlServerTestBlankFile()
        {
            TestBlankFile(Flavor.SqlServer);
        }

        [Test]
        public void PostgresTestBlankFile()
        {
            TestBlankFile(Flavor.Postgres);
        }

        [Test]
        public void MySqlTestBlankFile()
        {
            TestBlankFile(Flavor.MySql);
        }

        [Test]
        public void MongoTestBlankFile()
        {
            TestBlankFile(Flavor.Mongo);
        }

        private void TestBlankFile(Flavor flavor)
        {
            string inputString = "";
            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>();
            Test(inputString, expectedSpecs, flavor);

            inputString = @"
";
            Test(inputString, expectedSpecs, flavor);
        }

        [Test]
        public void SqlServerTestBareWildcard()
        {
            TestBareWildcard(Flavor.SqlServer);
        }

        [Test]
        public void PostgresTestBareWildcard()
        {
            TestBareWildcard(Flavor.Postgres);
        }

        [Test]
        public void MySqlTestBareWildcard()
        {
            TestBareWildcard(Flavor.MySql);
        }

        [Test]
        public void MongoTestBareWildcard()
        {
            TestBareWildcard(Flavor.Mongo);
        }

        private void TestBareWildcard(Flavor flavor)
        {
            // * should be considered *.*
            string inputString = @"
*
-*";

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(schema: TableSpecificationFragment.Star, table: TableSpecificationFragment.Star, negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
                new TableWithSchemaSpecificationWithCustomSelect(schema: TableSpecificationFragment.Star, table: TableSpecificationFragment.Star, negated: true, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
            };

            Test(inputString, expectedSpecs, flavor);
        }

        private void TestNegation(Flavor flavor)
        {
            /* @"
- HelloWorld
-[Hello].World
"
*/
            
            StringBuilder inputStringBuilder = new StringBuilder(@"
- HelloWorld
");
            if (flavor.SchemasSupported)
            {
                inputStringBuilder.AppendLine(string.Format("-{0}Hello{1}.World", flavor.OpeningQuoteChar, flavor.ClosingQuoteChar));
            }

            string inputString = inputStringBuilder.ToString();

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(schema: null, table: Frag("HelloWorld"), negated: true, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive),
            };

            if (flavor.SchemasSupported)
            {
                expectedSpecs.Add(new TableWithSchemaSpecificationWithCustomSelect(schema: Frag("Hello"), table: Frag("World"), negated: true, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive));
            }

            Test(inputString, expectedSpecs, flavor);
        }

        [Test]
        public void SqlServerTestNegation()
        {
            TestNegation(Flavor.SqlServer);
        }

        [Test]
        public void PostgresTestNegation()
        {
            TestNegation(Flavor.Postgres);
        }

        [Test]
        public void MySqlTestNegation()
        {
            TestNegation(Flavor.MySql);
        }

        [Test]
        public void MongoTestNegation()
        {
            TestNegation(Flavor.Mongo);
        }

        private void TestCustomSelect(Flavor flavor)
        {
            StringBuilder inputStringBuilder = new StringBuilder(@"
HelloWorld : SELECT * FROM HelloWorld WHERE IncludeInImport = 1
");
            if (flavor.SchemasSupported)
            {
                inputStringBuilder.AppendLine(string.Format("Hello.{0}World{1}:SELECT * FROM Hello.{0}World{1} WHERE IncludeInImport = 1", flavor.OpeningQuoteChar, flavor.ClosingQuoteChar));
            }

            string inputString = inputStringBuilder.ToString();

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(schema: null, table: Frag("HelloWorld"), negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive, customSelect: "SELECT * FROM HelloWorld WHERE IncludeInImport = 1")
            };

            if (flavor.SchemasSupported)
            {
                expectedSpecs.Add(new TableWithSchemaSpecificationWithCustomSelect(schema: Frag("Hello"), table: Frag("World"), negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive, customSelect: string.Format("SELECT * FROM Hello.{0}World{1} WHERE IncludeInImport = 1", flavor.OpeningQuoteChar, flavor.ClosingQuoteChar)));
            }

            Test(inputString, expectedSpecs, flavor);
        }

        [Test]
        public void SqlServerTestCustomSelect()
        {
            TestCustomSelect(Flavor.SqlServer);
        }

        [Test]
        public void PostgresTestCustomSelect()
        {
            TestCustomSelect(Flavor.Postgres);
        }

        [Test]
        public void MySqlTestCustomSelectThrows()
        {
            string inputString = "tab : SELECT * FROM tab";
            TestThrows<TableSpecificationParseException>(inputString, Flavor.MySql);
        }

        [Test]
        public void MongoTestCustomSelectThrows()
        {
            string inputString = "collection : would would even go here?";
            TestThrows<TableSpecificationParseException>(inputString, Flavor.Mongo);
        }

        private void TestSyntaxErrorThrows(string inputString, Flavor flavor)
        {
            TestThrows<TableSpecificationParseException>(inputString, flavor);
        }

        [Test]
        public void SqlServerTestSyntaxErrorThrows()
        {
            TestSyntaxErrorThrows("PeriodAtEnd.", Flavor.SqlServer);
        }

        [Test]
        public void PostgresTestSyntaxErrorThrows()
        {
            TestSyntaxErrorThrows("PeriodAtEnd.", Flavor.Postgres);
        }

        [Test]
        public void MySqlTestSyntaxErrorThrows()
        {
            TestSyntaxErrorThrows("PeriodAtEnd.", Flavor.MySql);
        }

        [Test]
        public void MongoTestSyntaxErrorThrows()
        {
            TestSyntaxErrorThrows("$$dollars", Flavor.Mongo);
        }

        [Test]
        public void MySqlTestSchemaThrows()
        {
            string inputString = "Sch.Tab";
            TestThrows<TableSpecificationParseException>(inputString, Flavor.MySql);

            inputString = "`Sch`.`Tab`";
            TestThrows<TableSpecificationParseException>(inputString, Flavor.MySql);

            inputString = "\"Sch\".\"ab\"";
            TestThrows<TableSpecificationParseException>(inputString, Flavor.MySql);
        }

        [Test]
        public void MongoTestDotIsOk()
        {
            Flavor flavor = Flavor.Mongo;
            
            string inputString = "abc.def";

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(null, Frag("abc.def"), negated: false, defaultSchemaIsCaseSensitive: flavor.DefaultSchemaCaseSensitive)
            };

            Test(inputString, expectedSpecs, flavor);
        }

        private TableSpecificationFragment Frag(string s)
        {
            return new TableSpecificationFragment(s, caseSensitive: false);
        }

        private TableSpecificationFragment Frag(string s, params object[] args)
        {
            return new TableSpecificationFragment(string.Format(s, args), caseSensitive: false);
        }

        private void Test(string inputString, List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs, Flavor flavor)
        {
            TableSpecListParser parser = new TableSpecListParser();
            using (StringReader input = new StringReader(inputString))
            {
                IList<TableWithSchemaSpecificationWithCustomSelect> actualSpecs = parser.Parse(input, flavor.Syntax, flavor.CustomSelectSupported, "test.txt");
                Assert.That(actualSpecs, Is.EqualTo(expectedSpecs).Using<TableWithSchemaSpecificationWithCustomSelect>(Comparisons.TableWithSchemaSpecificationWithCustomSelectComparison));
            }
        }

        private void TestThrows<TException>(string inputString, Flavor flavor)
        {
            TableSpecListParser parser = new TableSpecListParser();
            using (StringReader input = new StringReader(inputString))
            {
                Assert.Throws(Is.InstanceOf<TException>(), () => parser.Parse(input, flavor.Syntax, flavor.CustomSelectSupported, "test.txt"));
            }
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
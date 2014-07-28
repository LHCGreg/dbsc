using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using dbsc.Core.Antlr;
using dbsc.Core.ImportTableSpecification;
using dbsc.Core.Tests.ImportTableSpecification;

namespace dbsc.SqlServer.Tests
{
    [TestFixture]
    public class TableWithSchemaSpecWithCustomSelectListParserFixture
    {        
        [Test]
        public void BasicTest()
        {
            TableWithSchemaSpecWithCustomSelectListParser parser = new TableWithSchemaSpecWithCustomSelectListParser();
            string inputString = @"
HelloWorld
[Hello World]

Hello . [World]
[Brackets]] are fun].[Really]]]]]]]]really fun]
[Let's have some * wildcards*].Regular*
[*].*
  ";

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(null, FragFromString("HelloWorld"), negated: false, defaultSchemaIsCaseSensitive: false),
                new TableWithSchemaSpecificationWithCustomSelect(null, FragFromString("Hello World"), negated: false, defaultSchemaIsCaseSensitive: false),
                new TableWithSchemaSpecificationWithCustomSelect(FragFromString("Hello"), FragFromString("World"), negated: false, defaultSchemaIsCaseSensitive: false),
                new TableWithSchemaSpecificationWithCustomSelect(FragFromString("Brackets] are fun"), FragFromString("Really]]]]really fun"), negated: false, defaultSchemaIsCaseSensitive: false),
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
                    negated: false, defaultSchemaIsCaseSensitive: false),
                new TableWithSchemaSpecificationWithCustomSelect(
                    new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star }, caseSensitive: false),
                    new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star }, caseSensitive: false),
                    negated: false, defaultSchemaIsCaseSensitive: false)
            };

            using (StringReader input = new StringReader(inputString))
            {
                IList<TableWithSchemaSpecificationWithCustomSelect> actualSpecs = parser.Parse(input, "test.txt");
                Assert.That(actualSpecs, Is.EqualTo(expectedSpecs).Using<TableWithSchemaSpecificationWithCustomSelect>(Comparisons.TableWithSchemaSpecificationWithCustomSelectComparison));
            }
        }

        [Test]
        public void TestNonblankFirstLine()
        {
            TableWithSchemaSpecWithCustomSelectListParser parser = new TableWithSchemaSpecWithCustomSelectListParser();

            string inputString = "HelloWorld";
            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(null, FragFromString("HelloWorld"), negated: false, defaultSchemaIsCaseSensitive: false),
            };

            using (StringReader input = new StringReader(inputString))
            {
                IList<TableWithSchemaSpecificationWithCustomSelect> actualSpecs = parser.Parse(input, "test.txt");
                Assert.That(actualSpecs, Is.EqualTo(expectedSpecs).Using<TableWithSchemaSpecificationWithCustomSelect>(Comparisons.TableWithSchemaSpecificationWithCustomSelectComparison));
            }
        }

        [Test]
        public void TestBlankFile()
        {
            TableWithSchemaSpecWithCustomSelectListParser parser = new TableWithSchemaSpecWithCustomSelectListParser();

            string inputString = "";
            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>();

            using (StringReader input = new StringReader(inputString))
            {
                IList<TableWithSchemaSpecificationWithCustomSelect> actualSpecs = parser.Parse(input, "test.txt");
                Assert.That(actualSpecs, Is.EqualTo(expectedSpecs).Using<TableWithSchemaSpecificationWithCustomSelect>(Comparisons.TableWithSchemaSpecificationWithCustomSelectComparison));
            }

            inputString = @"
";

            using (StringReader input = new StringReader(inputString))
            {
                IList<TableWithSchemaSpecificationWithCustomSelect> actualSpecs = parser.Parse(input, "test.txt");
                Assert.That(actualSpecs, Is.EqualTo(expectedSpecs).Using<TableWithSchemaSpecificationWithCustomSelect>(Comparisons.TableWithSchemaSpecificationWithCustomSelectComparison));
            }
        }

        [Test]
        public void TestBareWildcard()
        {
            // * should be considered *.*
            TableWithSchemaSpecWithCustomSelectListParser parser = new TableWithSchemaSpecWithCustomSelectListParser();

            string inputString = @"
*
-*";

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(schema: TableSpecificationFragment.Star, table: TableSpecificationFragment.Star, negated: false, defaultSchemaIsCaseSensitive: false),
                new TableWithSchemaSpecificationWithCustomSelect(schema: TableSpecificationFragment.Star, table: TableSpecificationFragment.Star, negated: true, defaultSchemaIsCaseSensitive: false),
            };

            using (StringReader input = new StringReader(inputString))
            {
                IList<TableWithSchemaSpecificationWithCustomSelect> actualSpecs = parser.Parse(input, "test.txt");
                Assert.That(actualSpecs, Is.EqualTo(expectedSpecs).Using<TableWithSchemaSpecificationWithCustomSelect>(Comparisons.TableWithSchemaSpecificationWithCustomSelectComparison));
            }
        }

        [Test]
        public void TestNegation()
        {
            TableWithSchemaSpecWithCustomSelectListParser parser = new TableWithSchemaSpecWithCustomSelectListParser();

            string inputString = @"
- HelloWorld
-[Hello].World
";

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(schema: null, table: new TableSpecificationFragment("HelloWorld", caseSensitive: false), negated: true, defaultSchemaIsCaseSensitive: false),
                new TableWithSchemaSpecificationWithCustomSelect(schema: new TableSpecificationFragment("Hello", caseSensitive: false), table: new TableSpecificationFragment("World", caseSensitive: false), negated: true, defaultSchemaIsCaseSensitive: false),
            };

            using (StringReader input = new StringReader(inputString))
            {
                IList<TableWithSchemaSpecificationWithCustomSelect> actualSpecs = parser.Parse(input, "test.txt");
                Assert.That(actualSpecs, Is.EqualTo(expectedSpecs).Using<TableWithSchemaSpecificationWithCustomSelect>(Comparisons.TableWithSchemaSpecificationWithCustomSelectComparison));
            }
        }

        [Test]
        public void TestCustomSelect()
        {
            TableWithSchemaSpecWithCustomSelectListParser parser = new TableWithSchemaSpecWithCustomSelectListParser();
            string inputString = @"
HelloWorld : SELECT * FROM HelloWorld WHERE IncludeInImport = 1
Hello.[World]:SELECT * FROM Hello.[World] WHERE IncludeInImport = 1
";

            List<TableWithSchemaSpecificationWithCustomSelect> expectedSpecs = new List<TableWithSchemaSpecificationWithCustomSelect>()
            {
                new TableWithSchemaSpecificationWithCustomSelect(schema: null, table: new TableSpecificationFragment("HelloWorld", caseSensitive: false), negated: false, defaultSchemaIsCaseSensitive: false, customSelect: "SELECT * FROM HelloWorld WHERE IncludeInImport = 1"),
                new TableWithSchemaSpecificationWithCustomSelect(schema: new TableSpecificationFragment("Hello", caseSensitive: false), table: new TableSpecificationFragment("World", caseSensitive: false), negated: false, defaultSchemaIsCaseSensitive: false, customSelect: "SELECT * FROM Hello.[World] WHERE IncludeInImport = 1"),
            };

            using (StringReader input = new StringReader(inputString))
            {
                IList<TableWithSchemaSpecificationWithCustomSelect> actualSpecs = parser.Parse(input, "test.txt");
                Assert.That(actualSpecs, Is.EqualTo(expectedSpecs).Using<TableWithSchemaSpecificationWithCustomSelect>(Comparisons.TableWithSchemaSpecificationWithCustomSelectComparison));
            }
        }

        [Test]
        public void TestSyntaxErrorThrows()
        {
            TableWithSchemaSpecWithCustomSelectListParser parser = new TableWithSchemaSpecWithCustomSelectListParser();
            string inputString = @"PeriodAtEnd.";
            using (StringReader input = new StringReader(inputString))
            {
                Assert.Throws(Is.InstanceOf<TableSpecificationParseException>(), () => parser.Parse(input, "test.txt"));
            }
        }

        private TableSpecificationFragment FragFromString(string s)
        {
            return new TableSpecificationFragment(s, caseSensitive: false);
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
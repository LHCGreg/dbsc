using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.ImportTableSpecification;
using NUnit.Framework;

namespace dbsc.Core.Tests.ImportTableSpecification
{
    [TestFixture]
    public class TableWithSchemaSpecificationCollectionFixture
    {
        private List<TableWithSchema> EligibleTables = new List<TableWithSchema>()
        {
            new TableWithSchema("dbo", "Table1"),
            new TableWithSchema("dbo", "Table2"),
            new TableWithSchema("MySchema", "Table1"),
            new TableWithSchema("MySchema", "Table2")
        };
        
        [Test]
        public void BasicTest()
        {
            TableWithSchemaSpecificationCollection<TableWithSchema> specs = new TableWithSchemaSpecificationCollection<TableWithSchema>(
                new List<TableWithSchemaSpecification>()
                {
                    // Table1
                    // My*.Table2
                    new TableWithSchemaSpecification(schema: null, table: new TableSpecificationFragment("Table1", caseSensitive: false), negated: false, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(
                        schema: new TableSpecificationFragment(new List<StringOrWildcard>()
                        {
                            new StringOrWildcard("My"),
                            StringOrWildcard.Star
                        }, caseSensitive: false),
                        table: new TableSpecificationFragment("Table2", caseSensitive: false),
                        negated: false,
                        defaultSchemaIsCaseSensitive: false)
                }, defaultSchema: "dbo");

            // Expected:
            // dbo.Table1
            // MySchema.Table2

            var expectedTables = new List<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>()
            {
                new TableAndRule<TableWithSchema, TableWithSchemaSpecification>(
                    new TableWithSchema("dbo", "Table1"), specs.TableSpecifications[0]),
                new TableAndRule<TableWithSchema, TableWithSchemaSpecification>(
                    new TableWithSchema("MySchema", "Table2"), specs.TableSpecifications[1])
            };

            var actualTables = specs.GetTablesToImport(EligibleTables);

            Assert.That(actualTables, Is.EquivalentTo(expectedTables).Using<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>(Comparisons.TableAndRuleComparision));
        }

        [Test]
        public void TestAllNegative()
        {
            // Test that a collection of only negative specifications gets treated as a blacklist
            
            TableWithSchemaSpecificationCollection<TableWithSchema> specs = new TableWithSchemaSpecificationCollection<TableWithSchema>(
                new List<TableWithSchemaSpecification>()
                {
                    // -Table1
                    // -MySchema.Table2
                    // -DoesNot.Exist
                    new TableWithSchemaSpecification(schema: null, table: new TableSpecificationFragment("Table1", caseSensitive: false), negated: true, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(schema: new TableSpecificationFragment("MySchema", caseSensitive: false), table: new TableSpecificationFragment("Table2", caseSensitive: false), negated: true, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(schema: new TableSpecificationFragment("DoesNot", caseSensitive: false), table: new TableSpecificationFragment("Exist", caseSensitive: false), negated: true, defaultSchemaIsCaseSensitive: false),
                }, defaultSchema: "dbo");

            // Expected:
            // dbo.Table2
            // MySchema.Table1

            var expectedTables = new List<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>()
            {
                new TableAndRule<TableWithSchema, TableWithSchemaSpecification>(
                    new TableWithSchema("dbo", "Table2"), TableWithSchemaSpecification.Star),
                new TableAndRule<TableWithSchema, TableWithSchemaSpecification>(
                    new TableWithSchema("MySchema", "Table1"), TableWithSchemaSpecification.Star)
            };

            var actualTables = specs.GetTablesToImport(EligibleTables);

            Assert.That(actualTables, Is.EquivalentTo(expectedTables).Using<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>(Comparisons.TableAndRuleComparision));
        }

        [Test]
        public void TestOnePartNames()
        {
            TableWithSchemaSpecificationCollection<TableWithSchema> specs = new TableWithSchemaSpecificationCollection<TableWithSchema>(
    new List<TableWithSchemaSpecification>()
                {
                    // Table1
                    // *2
                    new TableWithSchemaSpecification(schema: null, table: new TableSpecificationFragment("Table1", caseSensitive: false), negated: false, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(schema: null,
                        table: new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star, new StringOrWildcard("2") }, caseSensitive: false),
                        negated: false, defaultSchemaIsCaseSensitive: false),
                }, defaultSchema: "dbo");

            // Expected:
            // dbo.Table1
            // dbo.Table2

            var expectedTables = new List<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>()
            {
                new TableAndRule<TableWithSchema, TableWithSchemaSpecification>(
                    new TableWithSchema("dbo", "Table1"), specs.TableSpecifications[0]),
                new TableAndRule<TableWithSchema, TableWithSchemaSpecification>(
                    new TableWithSchema("dbo", "Table2"), specs.TableSpecifications[1])
            };

            var actualTables = specs.GetTablesToImport(EligibleTables);

            Assert.That(actualTables, Is.EquivalentTo(expectedTables).Using<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>(Comparisons.TableAndRuleComparision));
        }

        [Test]
        public void TestDefaultSchemaCaseSensitivity()
        {
                        TableWithSchemaSpecificationCollection<TableWithSchema> specs = new TableWithSchemaSpecificationCollection<TableWithSchema>(
    new List<TableWithSchemaSpecification>()
                {
                    // Table1
                    // *2
                    new TableWithSchemaSpecification(schema: null, table: new TableSpecificationFragment("Table1", caseSensitive: false), negated: false, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(schema: null,
                        table: new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star, new StringOrWildcard("2") }, caseSensitive: false),
                        negated: false, defaultSchemaIsCaseSensitive: false),
                }, defaultSchema: "DBO");

            var expectedTables = new List<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>()
            {
                new TableAndRule<TableWithSchema, TableWithSchemaSpecification>(
                    new TableWithSchema("dbo", "Table1"), specs.TableSpecifications[0]),
                new TableAndRule<TableWithSchema, TableWithSchemaSpecification>(
                    new TableWithSchema("dbo", "Table2"), specs.TableSpecifications[1])
            };

            var actualTables = specs.GetTablesToImport(EligibleTables);

            Assert.That(actualTables, Is.EquivalentTo(expectedTables).Using<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>(Comparisons.TableAndRuleComparision));


            specs = new TableWithSchemaSpecificationCollection<TableWithSchema>(
                new List<TableWithSchemaSpecification>()
                {
                    // Table1
                    // *2
                    new TableWithSchemaSpecification(schema: null, table: new TableSpecificationFragment("Table1", caseSensitive: true), negated: false, defaultSchemaIsCaseSensitive: true),
                    new TableWithSchemaSpecification(schema: null,
                        table: new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star, new StringOrWildcard("2") }, caseSensitive: true),
                        negated: false, defaultSchemaIsCaseSensitive: true),
                }, defaultSchema: "DBO");

            expectedTables = new List<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>();

            actualTables = specs.GetTablesToImport(EligibleTables);

            Assert.That(actualTables, Is.EquivalentTo(expectedTables).Using<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>(Comparisons.TableAndRuleComparision));
        }

        [Test]
        public void TestFragmentCaseSensitivity()
        {
            TableWithSchemaSpecificationCollection<TableWithSchema> specs = new TableWithSchemaSpecificationCollection<TableWithSchema>(
new List<TableWithSchemaSpecification>()
                {
                    // Table1
                    // *e2
                    new TableWithSchemaSpecification(schema: null, table: new TableSpecificationFragment("TABLE1", caseSensitive: true), negated: false, defaultSchemaIsCaseSensitive: true),
                    new TableWithSchemaSpecification(schema: null,
                        table: new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star, new StringOrWildcard("E2") }, caseSensitive: true),
                        negated: false, defaultSchemaIsCaseSensitive: true),
                }, defaultSchema: "dbo");

            var expectedTables = new List<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>();

            var actualTables = specs.GetTablesToImport(EligibleTables);

            Assert.That(actualTables, Is.EquivalentTo(expectedTables).Using<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>(Comparisons.TableAndRuleComparision));
        }

        [Test]
        public void TestLastMatchWins()
        {
            TableWithSchemaSpecificationCollection<TableWithSchema> specs = new TableWithSchemaSpecificationCollection<TableWithSchema>(
    new List<TableWithSchemaSpecification>()
                {
                    // *.Table1
                    // -Table1
                    // -MySchema.*
                    // MySchema.Table1
                    new TableWithSchemaSpecification(schema: TableSpecificationFragment.Star, table: new TableSpecificationFragment("Table1", caseSensitive: false), negated: false, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(schema: null, table: new TableSpecificationFragment("Table1", caseSensitive: false), negated: true, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(schema: new TableSpecificationFragment("MySchema", caseSensitive: false), table: TableSpecificationFragment.Star, negated: true, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(schema: new TableSpecificationFragment("MySchema", caseSensitive: false), table: new TableSpecificationFragment("Table1", caseSensitive: false), negated: false, defaultSchemaIsCaseSensitive: false),
                }, defaultSchema: "dbo");

            // Expected:
            // MySchema.Table1

            var expectedTables = new List<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>()
            {
                new TableAndRule<TableWithSchema, TableWithSchemaSpecification>(
                    new TableWithSchema("MySchema", "Table1"), specs.TableSpecifications[3]),
            };

            var actualTables = specs.GetTablesToImport(EligibleTables);

            Assert.That(actualTables, Is.EquivalentTo(expectedTables).Using<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>(Comparisons.TableAndRuleComparision));

        }

        [Test]
        public void TestEmpty()
        {
            // Test that an empty list of specifications returns an empty list of tables to import
            TableWithSchemaSpecificationCollection<TableWithSchema> specs = new TableWithSchemaSpecificationCollection<TableWithSchema>(
                new List<TableWithSchemaSpecification>()
                {
                    
                }, defaultSchema: "dbo");

            List<TableAndRule<TableWithSchema, TableWithSchemaSpecification>> expectedTables = new List<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>();

            ICollection<TableAndRule<TableWithSchema, TableWithSchemaSpecification>> actualTables = specs.GetTablesToImport(EligibleTables);

            Assert.That(actualTables, Is.EquivalentTo(expectedTables).Using<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>(Comparisons.TableAndRuleComparision));
        }

        [Test]
        public void TestGetNonWildcardTablesThatDontExist()
        {
            TableWithSchemaSpecificationCollection<TableWithSchema> specs = new TableWithSchemaSpecificationCollection<TableWithSchema>(
    new List<TableWithSchemaSpecification>()
                {
                    // Table1
                    // MySchema2.*
                    // -NegDoesNot.Exist
                    // DoesNot.Exist
                    new TableWithSchemaSpecification(schema: null, table: new TableSpecificationFragment("Table1", caseSensitive: false), negated: true, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(schema: new TableSpecificationFragment("MySchema2", caseSensitive: false), table: TableSpecificationFragment.Star, negated: true, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(schema: new TableSpecificationFragment("NegDoesNot", caseSensitive: false), table: new TableSpecificationFragment("Exist", caseSensitive: false), negated: true, defaultSchemaIsCaseSensitive: false),
                    new TableWithSchemaSpecification(schema: new TableSpecificationFragment("DoesNot", caseSensitive: false), table: new TableSpecificationFragment("Exist", caseSensitive: false), negated: false, defaultSchemaIsCaseSensitive: false),
                }, defaultSchema: "dbo");

            List<TableWithSchemaSpecification> expectedSpecs = new List<TableWithSchemaSpecification>()
            {
                new TableWithSchemaSpecification(schema: new TableSpecificationFragment("DoesNot", caseSensitive: false), table: new TableSpecificationFragment("Exist", caseSensitive: false), negated: false, defaultSchemaIsCaseSensitive: false),
            };

            ICollection<TableWithSchemaSpecification> actualSpecs = specs.GetNonWildcardTableSpecsThatDontExist(EligibleTables);

            Assert.That(actualSpecs, Is.EquivalentTo(expectedSpecs).Using<TableWithSchemaSpecification>(Comparisons.TableWithSchemaSpecificationComparision));
        }

        [Test]
        public void TestLongList()
        {
            List<TableWithSchema> eligibleTables = new List<TableWithSchema>();
            List<TableWithSchemaSpecification> specList = new List<TableWithSchemaSpecification>();
            var expectedTables = new List<TableAndRule<TableWithSchema,TableWithSchemaSpecification>>();
            for (int i = 0; i < 1000; i++)
            {
                eligibleTables.Add(new TableWithSchema("dbo", "Table" + i.ToString()));

                specList.Add(new TableWithSchemaSpecification(
                    new TableSpecificationFragment("dbo", caseSensitive: false),
                    new TableSpecificationFragment("Table" + i.ToString(),caseSensitive: false),
                    negated: false,
                    defaultSchemaIsCaseSensitive: false));

                expectedTables.Add(new TableAndRule<TableWithSchema, TableWithSchemaSpecification>(eligibleTables[i], specList[i]));
            }

            TableWithSchemaSpecificationCollection<TableWithSchema> specs = new TableWithSchemaSpecificationCollection<TableWithSchema>(specList, "dbo");

            ICollection<TableAndRule<TableWithSchema, TableWithSchemaSpecification>> actualTables = specs.GetTablesToImport(eligibleTables);

            Assert.That(actualTables, Is.EquivalentTo(expectedTables).Using<TableAndRule<TableWithSchema, TableWithSchemaSpecification>>(Comparisons.TableAndRuleComparision));
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
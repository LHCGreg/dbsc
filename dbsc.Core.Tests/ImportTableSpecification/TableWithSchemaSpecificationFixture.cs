using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.ImportTableSpecification;
using NUnit.Framework;

namespace dbsc.Core.Tests.ImportTableSpecification
{
    [TestFixture]
    public class TableWithSchemaSpecificationFixture
    {
        private TableSpecificationFragment SchemaFrag = new TableSpecificationFragment(new List<StringOrWildcard>()
        {
            new StringOrWildcard("dbo")
        }, caseSensitive: false);
        
        private TableSpecificationFragment TableFrag = new TableSpecificationFragment(new List<StringOrWildcard>()
        {
            new StringOrWildcard("abc"),
            StringOrWildcard.Star
        }, caseSensitive: false);
        
        [Test]
        public void TestNoSchema()
        {
            TableWithSchemaSpecification spec = new TableWithSchemaSpecification(schema: null, table: TableFrag, negated: false, defaultSchemaIsCaseSensitive: false);
            Assert.That(spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.PositiveMatch));
            Assert.That(spec.Match(new TableWithSchema("dbo", "ab"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.NoMatch));
            Assert.That(spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "MySchema"), Is.EqualTo(MatchResult.NoMatch));

            spec = new TableWithSchemaSpecification(schema: null, table: TableFrag, negated: true, defaultSchemaIsCaseSensitive: false);
            Assert.That(spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.NegativeMatch));
            Assert.That(spec.Match(new TableWithSchema("dbo", "ab"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.NoMatch));
            Assert.That(spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "MySchema"), Is.EqualTo(MatchResult.NoMatch));
        }

        [Test]
        public void TestCaseSensitivity()
        {
            TableWithSchemaSpecification spec = new TableWithSchemaSpecification(schema: null, table: TableFrag, negated: false, defaultSchemaIsCaseSensitive: false);
            Assert.That(spec.Match(new TableWithSchema("DBO", "ABCD"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.PositiveMatch));

            spec = new TableWithSchemaSpecification(schema: null, table: TableFrag, negated: false, defaultSchemaIsCaseSensitive: true);
            Assert.That(spec.Match(new TableWithSchema("DBO", "ABCD"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.NoMatch));
        }

        [Test]
        public void TestWithSchema()
        {
            TableWithSchemaSpecification spec = new TableWithSchemaSpecification(schema: SchemaFrag, table: TableFrag, negated: false, defaultSchemaIsCaseSensitive: false);
            Assert.That(spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.PositiveMatch));
            Assert.That(spec.Match(new TableWithSchema("dbo", "ab"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.NoMatch));
            Assert.That(spec.Match(new TableWithSchema("MySchema", "abcd"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.NoMatch));

            spec = new TableWithSchemaSpecification(schema: SchemaFrag, table: TableFrag, negated: true, defaultSchemaIsCaseSensitive: false);
            Assert.That(spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.NegativeMatch));
            Assert.That(spec.Match(new TableWithSchema("dbo", "ab"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.NoMatch));
            Assert.That(spec.Match(new TableWithSchema("MySchema", "abcd"), defaultSchema: "dbo"), Is.EqualTo(MatchResult.NoMatch));
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
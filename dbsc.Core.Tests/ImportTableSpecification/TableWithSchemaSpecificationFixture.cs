using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.ImportTableSpecification;
using Xunit;

namespace dbsc.Core.Tests.ImportTableSpecification
{
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
        
        [Fact]
        public void TestNoSchema()
        {
            TableWithSchemaSpecification spec = new TableWithSchemaSpecification(schema: null, table: TableFrag, negated: false, defaultSchemaIsCaseSensitive: false);
            Assert.Equal(MatchResult.PositiveMatch, spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "dbo"));
            Assert.Equal(MatchResult.NoMatch, spec.Match(new TableWithSchema("dbo", "ab"), defaultSchema: "dbo"));
            Assert.Equal(MatchResult.NoMatch, spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "MySchema"));

            spec = new TableWithSchemaSpecification(schema: null, table: TableFrag, negated: true, defaultSchemaIsCaseSensitive: false);
            Assert.Equal(MatchResult.NegativeMatch, spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "dbo"));
            Assert.Equal(MatchResult.NoMatch, spec.Match(new TableWithSchema("dbo", "ab"), defaultSchema: "dbo"));
            Assert.Equal(MatchResult.NoMatch, spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "MySchema"));
        }

        [Fact]
        public void TestCaseSensitivity()
        {
            TableWithSchemaSpecification spec = new TableWithSchemaSpecification(schema: null, table: TableFrag, negated: false, defaultSchemaIsCaseSensitive: false);
            Assert.Equal(MatchResult.PositiveMatch, spec.Match(new TableWithSchema("DBO", "ABCD"), defaultSchema: "dbo"));

            spec = new TableWithSchemaSpecification(schema: null, table: TableFrag, negated: false, defaultSchemaIsCaseSensitive: true);
            Assert.Equal(MatchResult.NoMatch, spec.Match(new TableWithSchema("DBO", "ABCD"), defaultSchema: "dbo"));
        }

        [Fact]
        public void TestWithSchema()
        {
            TableWithSchemaSpecification spec = new TableWithSchemaSpecification(schema: SchemaFrag, table: TableFrag, negated: false, defaultSchemaIsCaseSensitive: false);
            Assert.Equal(MatchResult.PositiveMatch, spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "dbo"));
            Assert.Equal(MatchResult.NoMatch, spec.Match(new TableWithSchema("dbo", "ab"), defaultSchema: "dbo"));
            Assert.Equal(MatchResult.NoMatch, spec.Match(new TableWithSchema("MySchema", "abcd"), defaultSchema: "dbo"));

            spec = new TableWithSchemaSpecification(schema: SchemaFrag, table: TableFrag, negated: true, defaultSchemaIsCaseSensitive: false);
            Assert.Equal(MatchResult.NegativeMatch, spec.Match(new TableWithSchema("dbo", "abcd"), defaultSchema: "dbo"));
            Assert.Equal(MatchResult.NoMatch, spec.Match(new TableWithSchema("dbo", "ab"), defaultSchema: "dbo"));
            Assert.Equal(MatchResult.NoMatch, spec.Match(new TableWithSchema("MySchema", "abcd"), defaultSchema: "dbo"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.ImportTableSpecification;
using Xunit;

namespace dbsc.Core.Tests.ImportTableSpecification
{
    public class TableSpecificationFragmentFixture
    {
        [Fact]
        public void TestNoWildcards()
        {
            TableSpecificationFragment frag = new TableSpecificationFragment(new List<StringOrWildcard>() { new StringOrWildcard("[abc") }, caseSensitive: false);
            Assert.True(frag.Matches("[abc"));
            Assert.True(frag.Matches("[ABc"));
            Assert.False(frag.Matches("abc"));
        }

        [Fact]
        public void TestNoWildcardsCaseSensitive()
        {
            TableSpecificationFragment frag = new TableSpecificationFragment(new List<StringOrWildcard>() { new StringOrWildcard("[abc") }, caseSensitive: true);
            Assert.True(frag.Matches("[abc"));
            Assert.False(frag.Matches("[ABc"));
            Assert.False(frag.Matches("abc"));
        }

        [Fact]
        public void TestOnlyWildcard()
        {
            TableSpecificationFragment frag = TableSpecificationFragment.Star;
            Assert.True(frag.Matches("[abc"));
            Assert.True(frag.Matches("[ABc"));
            Assert.True(frag.Matches("abc"));
        }

        [Fact]
        public void TestWildcard()
        {
            // *abc
            TableSpecificationFragment frag = new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star, new StringOrWildcard("abc") }, caseSensitive: false);
            Assert.True(frag.Matches("[abc"));
            Assert.True(frag.Matches("[ABc"));
            Assert.True(frag.Matches("abc"));
            Assert.False(frag.Matches("abcd"));
            Assert.True(frag.Matches("abcabcabc"));
        }

        [Fact]
        public void TestMultipleWildcards()
        {
            // ab*c*d
            TableSpecificationFragment frag = new TableSpecificationFragment(new List<StringOrWildcard>()
            {
                new StringOrWildcard("ab"),
                StringOrWildcard.Star,
                new StringOrWildcard("c"),
                StringOrWildcard.Star,
                new StringOrWildcard("d")
            }, caseSensitive: false);

            Assert.False(frag.Matches("abc"));
            Assert.True(frag.Matches("abcd"));
            Assert.False(frag.Matches("aabcd"));
            Assert.True(frag.Matches("ab  c  D"));
            Assert.False(frag.Matches("ab  c  D "));
        }

        [Fact]
        public void TestMultipleWildcardsCaseSensitive()
        {
            // ab*c*d
            TableSpecificationFragment frag = new TableSpecificationFragment(new List<StringOrWildcard>()
            {
                new StringOrWildcard("ab"),
                StringOrWildcard.Star,
                new StringOrWildcard("c"),
                StringOrWildcard.Star,
                new StringOrWildcard("d")
            }, caseSensitive: true);

            Assert.False(frag.Matches("abc"));
            Assert.True(frag.Matches("abcd"));
            Assert.False(frag.Matches("aabcd"));
            Assert.False(frag.Matches("ab  c  D"));
            Assert.False(frag.Matches("ab  c  d "));
        }

        [Fact]
        public void TestHasWildcards()
        {
            TableSpecificationFragment noWildcards = new TableSpecificationFragment("[abc", caseSensitive: false);
            Assert.False(noWildcards.HasWildcards);

            noWildcards = new TableSpecificationFragment(new List<StringOrWildcard>() { new StringOrWildcard("[abc") }, caseSensitive: false);
            Assert.False(noWildcards.HasWildcards);

            TableSpecificationFragment wildcards = new TableSpecificationFragment(new List<StringOrWildcard>()
            {
                new StringOrWildcard("a"),
                StringOrWildcard.Star,
                new StringOrWildcard("b")
            }, caseSensitive: false);

            Assert.True(wildcards.HasWildcards);
        }
    }
}

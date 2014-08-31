using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dbsc.Core.ImportTableSpecification;
using NUnit.Framework;

namespace dbsc.Core.Tests.ImportTableSpecification
{
    [TestFixture]
    public class TableSpecificationFragmentFixture
    {
        [Test]
        public void TestNoWildcards()
        {
            TableSpecificationFragment frag = new TableSpecificationFragment(new List<StringOrWildcard>() { new StringOrWildcard("[abc") }, caseSensitive: false);
            Assert.That(frag.Matches("[abc"), Is.True);
            Assert.That(frag.Matches("[ABc"), Is.True);
            Assert.That(frag.Matches("abc"), Is.False);
        }

        [Test]
        public void TestNoWildcardsCaseSensitive()
        {
            TableSpecificationFragment frag = new TableSpecificationFragment(new List<StringOrWildcard>() { new StringOrWildcard("[abc") }, caseSensitive: true);
            Assert.That(frag.Matches("[abc"), Is.True);
            Assert.That(frag.Matches("[ABc"), Is.False);
            Assert.That(frag.Matches("abc"), Is.False);
        }

        [Test]
        public void TestOnlyWildcard()
        {
            TableSpecificationFragment frag = TableSpecificationFragment.Star;
            Assert.That(frag.Matches("[abc"), Is.True);
            Assert.That(frag.Matches("[ABc"), Is.True);
            Assert.That(frag.Matches("abc"), Is.True);
        }

        [Test]
        public void TestWildcard()
        {
            // *abc
            TableSpecificationFragment frag = new TableSpecificationFragment(new List<StringOrWildcard>() { StringOrWildcard.Star, new StringOrWildcard("abc") }, caseSensitive: false);
            Assert.That(frag.Matches("[abc"), Is.True);
            Assert.That(frag.Matches("[ABc"), Is.True);
            Assert.That(frag.Matches("abc"), Is.True);
            Assert.That(frag.Matches("abcd"), Is.False);
            Assert.That(frag.Matches("abcabcabc"), Is.True);
        }

        [Test]
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

            Assert.That(frag.Matches("abc"), Is.False);
            Assert.That(frag.Matches("abcd"), Is.True);
            Assert.That(frag.Matches("aabcd"), Is.False);
            Assert.That(frag.Matches("ab  c  D"), Is.True);
            Assert.That(frag.Matches("ab  c  D "), Is.False);
        }

        [Test]
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

            Assert.That(frag.Matches("abc"), Is.False);
            Assert.That(frag.Matches("abcd"), Is.True);
            Assert.That(frag.Matches("aabcd"), Is.False);
            Assert.That(frag.Matches("ab  c  D"), Is.False);
            Assert.That(frag.Matches("ab  c  d "), Is.False);
        }

        [Test]
        public void TestHasWildcards()
        {
            TableSpecificationFragment noWildcards = new TableSpecificationFragment("[abc", caseSensitive: false);
            Assert.That(noWildcards.HasWildcards, Is.False);

            noWildcards = new TableSpecificationFragment(new List<StringOrWildcard>() { new StringOrWildcard("[abc") }, caseSensitive: false);
            Assert.That(noWildcards.HasWildcards, Is.False);

            TableSpecificationFragment wildcards = new TableSpecificationFragment(new List<StringOrWildcard>()
            {
                new StringOrWildcard("a"),
                StringOrWildcard.Star,
                new StringOrWildcard("b")
            }, caseSensitive: false);

            Assert.That(wildcards.HasWildcards, Is.True);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace dbsc.Core.ImportTableSpecification
{
    /// <summary>
    /// Specification for one part of a table name. For a SQL Server table for example, you could have one fragment
    /// for the schema and one for the table name.
    /// </summary>
    public class TableSpecificationFragment
    {
        // This class is implemented as list of strings or wildcards. The abc*123 would have three things in its pattern,
        // "abc", StringOrWildcard.Star, "123"
        public IList<StringOrWildcard> Pattern { get; private set; }
        private bool _caseSensitive;
        
        // If the pattern has wildcards, make a regex. If not, just do a string compare.
        // _regex is null if it's just a string. _justAString is null if there are wildcards
        private Regex _regex;
        private string _justAString;

        public TableSpecificationFragment(string identifierWithoutWildcards, bool caseSensitive)
            : this(new List<StringOrWildcard>(1) { new StringOrWildcard(identifierWithoutWildcards) }, caseSensitive)
        {

        }

        public TableSpecificationFragment(IList<StringOrWildcard> pattern, bool caseSensitive)
        {
            Pattern = pattern;
            _caseSensitive = caseSensitive;

            if (pattern.Count == 1 && pattern[0].Type == WildcardType.None)
            {
                _justAString = pattern[0].String;
            }
            else
            {
                _regex = BuildRegex(pattern, caseSensitive);
            }
        }

        public bool HasWildcards { get { return _justAString == null; } }

        private static Regex BuildRegex(IList<StringOrWildcard> pattern, bool caseSensitive)
        {
            StringBuilder regexString = new StringBuilder();
            
            // Anchor to the beginning and end of the string
            regexString.Append("^");
            foreach (StringOrWildcard chunk in pattern)
            {
                regexString.Append(chunk.ToRegexString());
            }
            regexString.Append("$");

            RegexOptions options = RegexOptions.Singleline;
            if (!caseSensitive)
            {
                options |= RegexOptions.IgnoreCase;
            }

            return new Regex(regexString.ToString(), options);
        }

        public static TableSpecificationFragment Star
        {
            get
            {
                return new TableSpecificationFragment(new List<StringOrWildcard>(1) { StringOrWildcard.Star }, caseSensitive: false);
            }
        }

        public bool Matches(string str)
        {
            if (_justAString != null)
            {
                // Will have to refactor this for other database engines that may no be case insensitive
                StringComparison comparison;
                if (_caseSensitive)
                    comparison = StringComparison.CurrentCulture;
                else
                    comparison = StringComparison.CurrentCultureIgnoreCase;
                
                return _justAString.Equals(str, comparison);
            }
            else
            {
                return _regex.IsMatch(str);
            }
        }

        public override string ToString()
        {
            // This is SQL Server specific, but it's just for viewing when debugging
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            foreach (StringOrWildcard chunk in Pattern)
            {
                if (chunk.Type == WildcardType.None)
                {
                    builder.Append(chunk.String.Replace("]", "]]"));
                }
                else if (chunk.Type == WildcardType.Star)
                {
                    builder.Append("*");
                }
            }
            builder.Append("]");
            return builder.ToString();
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
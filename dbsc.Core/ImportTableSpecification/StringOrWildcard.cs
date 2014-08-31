using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace dbsc.Core.ImportTableSpecification
{
    public struct StringOrWildcard : IEquatable<StringOrWildcard>
    {
        /// <summary>
        /// null if Type is not None
        /// </summary>
        public readonly string String;
        public readonly WildcardType Type;

        public StringOrWildcard(string str)
        {
            String = str;
            Type = WildcardType.None;
        }

        private StringOrWildcard(string str, WildcardType type)
        {
            String = str;
            Type = type;
        }

        public static StringOrWildcard Star { get { return new StringOrWildcard(null, WildcardType.Star); } }

        internal string ToRegexString()
        {
            if (Type == WildcardType.None)
            {
                return Regex.Escape(String);
            }
            else if (Type == WildcardType.Star)
            {
                return ".*";
            }
            else
            {
                throw new Exception("Oops, missed a wildcard type.");
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StringOrWildcard)) return false;
            return Equals((StringOrWildcard)obj);
        }

        public bool Equals(StringOrWildcard other)
        {
            if (this.Type != other.Type) return false;
            if (this.Type == WildcardType.None && !this.String.Equals(other.String, StringComparison.Ordinal)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 23;
            unchecked
            {
                hash += 17 * this.Type.GetHashCode();
                if (this.String != null)
                {
                    hash += this.String.GetHashCode();
                }
            }
            return hash;
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
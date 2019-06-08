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

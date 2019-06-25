using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestUtils.Sql
{
    public class script_isolation_test : IEquatable<script_isolation_test>
    {
        public int step { get; set; }
        public string val { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as script_isolation_test);
        }

        public bool Equals(script_isolation_test other)
        {
            if (other == null) return false;
            return this.step == other.step && this.val.Equals(other.val, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash += 31 * step;
                hash += 31 * val.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", step, val);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestUtils.Sql
{
    public class Person : IEquatable<Person>
    {
        public int person_id { get; set; }
        public string name { get; set; }
        public DateTime birthday { get; set; }
        public int? default_test { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Person);
        }

        public bool Equals(Person other)
        {
            if (other == null) return false;
            return string.Equals(this.name, other.name, StringComparison.Ordinal) && this.birthday == other.birthday
                && this.default_test == other.default_test;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash += 31 * name.GetHashCode();
                hash += 31 * birthday.GetHashCode();
                hash += 31 * default_test.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", person_id, name, birthday.ToString("yyyy-MM-dd"), default_test);
        }
    }
}

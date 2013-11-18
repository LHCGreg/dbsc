using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Mongo.Integration
{
    [BsonIgnoreExtraElements]
    class PersonPreferences : IEquatable<PersonPreferences>
    {
        public int a { get; set; }
        public IList<int> b { get; set; }
        public bool c { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as PersonPreferences);
        }

        public bool Equals(PersonPreferences other)
        {
            if (other == null) return false;

            return this.a == other.a && BEqual(other) && this.c == other.c;
        }

        private bool BEqual(PersonPreferences other)
        {
            return (this.b != null && other.b != null && this.b.SequenceEqual(other.b))
                || (this.b == null && other.b == null);
        }

        public override int GetHashCode()
        {
            return a + c.GetHashCode() + (b != null ? b.Sum(x => x) : 0);
        }
    }
}

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Mongo.Integration
{
    [BsonIgnoreExtraElements]
    class Person : IEquatable<Person>
    {
        public string name { get; set; }
        public PersonPreferences preferences { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Person);
        }

        public bool Equals(Person other)
        {
            if (other == null) return false;
            return string.Equals(this.name, other.name, StringComparison.Ordinal) && this.preferences.Equals(other.preferences);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() + preferences.GetHashCode();
        }
    }
}

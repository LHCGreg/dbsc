using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Mongo.Integration
{
    [BsonIgnoreExtraElements]
    class Number : IEquatable<Number>
    {
        public int num { get; set; }
        public string english { get; set; }
        public string spanish { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Number);
        }

        public bool Equals(Number other)
        {
            if (other == null) return false;
            return this.num == other.num && string.Equals(this.english, other.english, StringComparison.Ordinal)
                && string.Equals(this.spanish, other.spanish, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return num + english.GetHashCode() + spanish.GetHashCode();
        }
    }
}

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Mongo.Integration
{
    [BsonIgnoreExtraElements]
    class Book : IEquatable<Book>
    {
        public string name { get; set; }
        public string author { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Book);
        }

        public bool Equals(Book other)
        {
            if (other == null) return false;
            return string.Equals(this.name, other.name, StringComparison.Ordinal) && string.Equals(this.author, other.author, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() + author.GetHashCode();
        }
    }
}

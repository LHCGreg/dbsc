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

/*
 Copyright 2013 Greg Najda

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
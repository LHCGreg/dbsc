﻿using MongoDB.Bson.Serialization.Attributes;
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
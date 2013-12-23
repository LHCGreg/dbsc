using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Postgres.Integration
{
    class Book : IEquatable<Book>
    {
        public int book_id { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public int author_person_id { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Book);
        }

        public bool Equals(Book other)
        {
            if (other == null) return false;
            return string.Equals(this.title, other.title, StringComparison.Ordinal) &&
                string.Equals(this.subtitle, other.subtitle, StringComparison.Ordinal)
                && this.author_person_id == other.author_person_id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash += 31 * title.GetHashCode();
                if (subtitle != null) hash += 31 * subtitle.GetHashCode();
                hash += 31 * author_person_id;
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", book_id, title, subtitle, author_person_id);
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
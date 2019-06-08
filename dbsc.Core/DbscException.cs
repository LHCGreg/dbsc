using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    /// <summary>
    /// Message is used as the error message verbatim, without showing inner exceptions or prefixing with "Error"
    /// </summary>
    [Serializable]
    public class DbscException : Exception
    {
        public DbscException() { }
        public DbscException(string message) : base(message) { }
        public DbscException(string message, Exception inner) : base(message, inner) { }
        protected DbscException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}

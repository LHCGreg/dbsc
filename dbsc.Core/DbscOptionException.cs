using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core
{
    [Serializable]
    public class DbscOptionException : DbscException
    {
        public bool ShowHelp { get; private set; }

        public DbscOptionException() { ShowHelp = false; }
        public DbscOptionException(bool showHelp) : base() { ShowHelp = showHelp; }
        public DbscOptionException(string message) : base(message) { ShowHelp = false; }
        public DbscOptionException(string message, bool showHelp) : base(message) { ShowHelp = showHelp; }
        public DbscOptionException(string message, Exception inner) : base(message, inner) { ShowHelp = false; }
        public DbscOptionException(string message, Exception inner, bool showHelp) : base(message, inner) { ShowHelp = showHelp; }
        protected DbscOptionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}

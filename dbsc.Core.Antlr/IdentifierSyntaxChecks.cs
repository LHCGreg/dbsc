using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Antlr
{
    static class IdentifierSyntaxChecks
    {
        public static bool TwoPartIdentifiersSupported(this IdentifierSyntax flavor)
        {
            switch (flavor)
            {
                case IdentifierSyntax.SqlServer:
                    return true;
                case IdentifierSyntax.Postgres:
                    return true;
                case IdentifierSyntax.MySql:
                    return false;
                case IdentifierSyntax.Mongo:
                    return false;
                default:
                    throw new Exception("Oops, missed checking if a database type supports two part identifiers.");
            }
        }
    }
}

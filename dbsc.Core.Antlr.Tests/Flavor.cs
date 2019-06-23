using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dbsc.Core.Antlr.Tests
{
    class Flavor
    {
        public IdentifierSyntax Syntax { get; private set; }
        public bool CustomSelectSupported { get; private set; }
        public bool DefaultSchemaCaseSensitive { get; private set; }
        public string OpeningQuoteChar { get; private set; }
        public string ClosingQuoteChar { get; private set; }
        public bool SchemasSupported { get; private set; }

        public Flavor(IdentifierSyntax syntax, bool customSelectSupported, bool schemasSuported, bool defaultSchemaCaseSensitive, string openingQuoteChar, string closingQuoteChar)
        {
            Syntax = syntax;
            CustomSelectSupported = customSelectSupported;
            SchemasSupported = schemasSuported;
            DefaultSchemaCaseSensitive = defaultSchemaCaseSensitive;
            OpeningQuoteChar = openingQuoteChar;
            ClosingQuoteChar = closingQuoteChar;
        }

        public static Flavor SqlServer
        {
            get
            {
                return new Flavor(IdentifierSyntax.SqlServer, customSelectSupported: true, schemasSuported: true, defaultSchemaCaseSensitive: false, openingQuoteChar: "[", closingQuoteChar: "]");
            }
        }

        public static Flavor Postgres
        {
            get
            {
                return new Flavor(IdentifierSyntax.Postgres, customSelectSupported: true, schemasSuported: true, defaultSchemaCaseSensitive: false, openingQuoteChar: "\"", closingQuoteChar: "\"");
            }
        }

        public static Flavor MySql
        {
            get
            {
                // MySQL does not have schemas but use false to match what is set by the parser.
                return new Flavor(IdentifierSyntax.MySql, customSelectSupported: false, schemasSuported: false, defaultSchemaCaseSensitive: false, openingQuoteChar: "`", closingQuoteChar: "`");
            }
        }

        public static Flavor Mongo
        {
            get
            {
                return new Flavor(IdentifierSyntax.Mongo, customSelectSupported: false, schemasSuported: false, defaultSchemaCaseSensitive: false, openingQuoteChar: null, closingQuoteChar: null);
            }
        }
    }
}

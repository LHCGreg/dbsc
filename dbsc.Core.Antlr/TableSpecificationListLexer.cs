//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.3
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from TableSpecificationList.g4 by ANTLR 4.3

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591

namespace dbsc.Core.Antlr {
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.3")]
[System.CLSCompliant(false)]
public partial class TableSpecificationListLexer : Lexer {
	public const int
		T__0=1, MS_UNENCLOSED_ID_NAME=2, MS_BRACKET_ENCLOSED_ID=3, PG_UNENCLOSED_ID_NAME=4, 
		PG_QUOTE_ENCLOSED_ID=5, MYSQL_UNENCLOSED_ID=6, MYSQL_BACKTICK_ID=7, MYSQL_QUOTE_ID=8, 
		MONGO_ID=9, WS_NO_NEWLINE=10, NEWLINE=11, LETTER=12, NUMBER=13, WILDCARD=14, 
		NEGATER=15, CUSTOM_SELECT=16;
	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] tokenNames = {
		"'\\u0000'", "'\\u0001'", "'\\u0002'", "'\\u0003'", "'\\u0004'", "'\\u0005'", 
		"'\\u0006'", "'\\u0007'", "'\b'", "'\t'", "'\n'", "'\\u000B'", "'\f'", 
		"'\r'", "'\\u000E'", "'\\u000F'", "'\\u0010'"
	};
	public static readonly string[] ruleNames = {
		"T__0", "MS_UNENCLOSED_ID_NAME", "MS_BRACKET_ENCLOSED_ID", "PG_UNENCLOSED_ID_NAME", 
		"PG_QUOTE_ENCLOSED_ID", "MYSQL_UNENCLOSED_ID", "MYSQL_BACKTICK_ID", "MYSQL_QUOTE_ID", 
		"MONGO_ID", "WS_NO_NEWLINE", "NEWLINE", "LETTER", "NUMBER", "WILDCARD", 
		"NEGATER", "CUSTOM_SELECT", "BACKSLASH_NEWLINE"
	};


		private IdentifierSyntax _flavor = IdentifierSyntax.SqlServer;
		public IdentifierSyntax Flavor { get { return _flavor; } set { _flavor = value; } }

		private bool _allowCustomSelect = true;
		public bool AllowCustomSelect { get { return _allowCustomSelect; } set { _allowCustomSelect = value; } }

		public TableSpecificationListLexer(ICharStream input, IdentifierSyntax flavor, bool allowCustomSelect)
			: this (input)
		{
			Flavor = flavor;
			AllowCustomSelect = allowCustomSelect;
		}


	public TableSpecificationListLexer(ICharStream input)
		: base(input)
	{
		_interp = new LexerATNSimulator(this,_ATN);
	}

	public override string GrammarFileName { get { return "TableSpecificationList.g4"; } }

	public override string[] TokenNames { get { return tokenNames; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return _serializedATN; } }

	public override bool Sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 1 : return MS_UNENCLOSED_ID_NAME_sempred(_localctx, predIndex);

		case 2 : return MS_BRACKET_ENCLOSED_ID_sempred(_localctx, predIndex);

		case 3 : return PG_UNENCLOSED_ID_NAME_sempred(_localctx, predIndex);

		case 4 : return PG_QUOTE_ENCLOSED_ID_sempred(_localctx, predIndex);

		case 5 : return MYSQL_UNENCLOSED_ID_sempred(_localctx, predIndex);

		case 6 : return MYSQL_BACKTICK_ID_sempred(_localctx, predIndex);

		case 7 : return MYSQL_QUOTE_ID_sempred(_localctx, predIndex);

		case 8 : return MONGO_ID_sempred(_localctx, predIndex);
		}
		return true;
	}
	private bool MYSQL_UNENCLOSED_ID_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 4: return Flavor == IdentifierSyntax.MySql;
		}
		return true;
	}
	private bool PG_QUOTE_ENCLOSED_ID_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 3: return Flavor == IdentifierSyntax.Postgres;
		}
		return true;
	}
	private bool PG_UNENCLOSED_ID_NAME_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 2: return Flavor == IdentifierSyntax.Postgres;
		}
		return true;
	}
	private bool MONGO_ID_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 7: return Flavor == IdentifierSyntax.Mongo;
		}
		return true;
	}
	private bool MYSQL_BACKTICK_ID_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 5: return Flavor == IdentifierSyntax.MySql;
		}
		return true;
	}
	private bool MS_BRACKET_ENCLOSED_ID_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 1: return Flavor == IdentifierSyntax.SqlServer;
		}
		return true;
	}
	private bool MYSQL_QUOTE_ID_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 6: return Flavor == IdentifierSyntax.MySql;
		}
		return true;
	}
	private bool MS_UNENCLOSED_ID_NAME_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0: return Flavor == IdentifierSyntax.SqlServer;
		}
		return true;
	}

	public static readonly string _serializedATN =
		"\x3\xAF6F\x8320\x479D\xB75C\x4880\x1605\x191C\xAB37\x2\x12\xAB\b\x1\x4"+
		"\x2\t\x2\x4\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6\x4\a\t\a\x4\b\t\b"+
		"\x4\t\t\t\x4\n\t\n\x4\v\t\v\x4\f\t\f\x4\r\t\r\x4\xE\t\xE\x4\xF\t\xF\x4"+
		"\x10\t\x10\x4\x11\t\x11\x4\x12\t\x12\x3\x2\x3\x2\x3\x3\x3\x3\x3\x3\x5"+
		"\x3+\n\x3\x3\x3\x3\x3\x3\x3\x3\x3\a\x3\x31\n\x3\f\x3\xE\x3\x34\v\x3\x3"+
		"\x3\x3\x3\x3\x4\x3\x4\x3\x4\x3\x4\x6\x4<\n\x4\r\x4\xE\x4=\x3\x4\x3\x4"+
		"\x3\x4\x3\x5\x3\x5\x3\x5\x5\x5\x46\n\x5\x3\x5\x3\x5\x3\x5\x3\x5\x3\x5"+
		"\a\x5M\n\x5\f\x5\xE\x5P\v\x5\x3\x5\x3\x5\x3\x6\x3\x6\x3\x6\x3\x6\x6\x6"+
		"X\n\x6\r\x6\xE\x6Y\x3\x6\x3\x6\x3\x6\x3\a\x3\a\x6\a\x61\n\a\r\a\xE\a\x62"+
		"\x3\a\x3\a\x3\b\x3\b\x3\b\x3\b\x6\bk\n\b\r\b\xE\bl\x3\b\x3\b\x3\b\x3\t"+
		"\x3\t\x3\t\x3\t\x6\tv\n\t\r\t\xE\tw\x3\t\x3\t\x3\t\x3\n\x3\n\x3\n\x5\n"+
		"\x80\n\n\x3\n\a\n\x83\n\n\f\n\xE\n\x86\v\n\x3\n\x3\n\x3\v\x3\v\x3\v\x3"+
		"\v\x3\f\x5\f\x8F\n\f\x3\f\x3\f\x3\r\x3\r\x3\xE\x3\xE\x3\xF\x3\xF\x3\x10"+
		"\x3\x10\x3\x11\x3\x11\a\x11\x9D\n\x11\f\x11\xE\x11\xA0\v\x11\x3\x11\x3"+
		"\x11\x3\x11\x6\x11\xA5\n\x11\r\x11\xE\x11\xA6\x3\x12\x3\x12\x3\x12\x2"+
		"\x2\x2\x13\x3\x2\x3\x5\x2\x4\a\x2\x5\t\x2\x6\v\x2\a\r\x2\b\xF\x2\t\x11"+
		"\x2\n\x13\x2\v\x15\x2\f\x17\x2\r\x19\x2\xE\x1B\x2\xF\x1D\x2\x10\x1F\x2"+
		"\x11!\x2\x12#\x2\x2\x3\x2\f\x5\x2%%\x42\x42\x61\x61\x5\x2\f\f\xF\xF^_"+
		"\x5\x2\f\f\xF\xF$$\b\x2&&\x32;\x43\\\x61\x61\x63|\x82\x1\x5\x2\f\f\xF"+
		"\xF\x62\x62\x5\x2\v\f\xF\xF\"\"\x4\x2\v\v\"\"\x5\x2\x43\\\x63|\x82\x101"+
		"\x3\x2\x32;\x5\x2\f\f\xF\xF^^\xC8\x2\x3\x3\x2\x2\x2\x2\x5\x3\x2\x2\x2"+
		"\x2\a\x3\x2\x2\x2\x2\t\x3\x2\x2\x2\x2\v\x3\x2\x2\x2\x2\r\x3\x2\x2\x2\x2"+
		"\xF\x3\x2\x2\x2\x2\x11\x3\x2\x2\x2\x2\x13\x3\x2\x2\x2\x2\x15\x3\x2\x2"+
		"\x2\x2\x17\x3\x2\x2\x2\x2\x19\x3\x2\x2\x2\x2\x1B\x3\x2\x2\x2\x2\x1D\x3"+
		"\x2\x2\x2\x2\x1F\x3\x2\x2\x2\x2!\x3\x2\x2\x2\x3%\x3\x2\x2\x2\x5*\x3\x2"+
		"\x2\x2\a\x37\x3\x2\x2\x2\t\x45\x3\x2\x2\x2\vS\x3\x2\x2\x2\r`\x3\x2\x2"+
		"\x2\xF\x66\x3\x2\x2\x2\x11q\x3\x2\x2\x2\x13\x7F\x3\x2\x2\x2\x15\x89\x3"+
		"\x2\x2\x2\x17\x8E\x3\x2\x2\x2\x19\x92\x3\x2\x2\x2\x1B\x94\x3\x2\x2\x2"+
		"\x1D\x96\x3\x2\x2\x2\x1F\x98\x3\x2\x2\x2!\x9A\x3\x2\x2\x2#\xA8\x3\x2\x2"+
		"\x2%&\a\x30\x2\x2&\x4\x3\x2\x2\x2\'+\x5\x19\r\x2(+\t\x2\x2\x2)+\x5\x1D"+
		"\xF\x2*\'\x3\x2\x2\x2*(\x3\x2\x2\x2*)\x3\x2\x2\x2+\x32\x3\x2\x2\x2,\x31"+
		"\x5\x19\r\x2-\x31\x5\x1B\xE\x2.\x31\t\x2\x2\x2/\x31\x5\x1D\xF\x2\x30,"+
		"\x3\x2\x2\x2\x30-\x3\x2\x2\x2\x30.\x3\x2\x2\x2\x30/\x3\x2\x2\x2\x31\x34"+
		"\x3\x2\x2\x2\x32\x30\x3\x2\x2\x2\x32\x33\x3\x2\x2\x2\x33\x35\x3\x2\x2"+
		"\x2\x34\x32\x3\x2\x2\x2\x35\x36\x6\x3\x2\x2\x36\x6\x3\x2\x2\x2\x37;\a"+
		"]\x2\x2\x38<\n\x3\x2\x2\x39:\a_\x2\x2:<\a_\x2\x2;\x38\x3\x2\x2\x2;\x39"+
		"\x3\x2\x2\x2<=\x3\x2\x2\x2=;\x3\x2\x2\x2=>\x3\x2\x2\x2>?\x3\x2\x2\x2?"+
		"@\a_\x2\x2@\x41\x6\x4\x3\x2\x41\b\x3\x2\x2\x2\x42\x46\x5\x19\r\x2\x43"+
		"\x46\a\x61\x2\x2\x44\x46\x5\x1D\xF\x2\x45\x42\x3\x2\x2\x2\x45\x43\x3\x2"+
		"\x2\x2\x45\x44\x3\x2\x2\x2\x46N\x3\x2\x2\x2GM\x5\x19\r\x2HM\a\x61\x2\x2"+
		"IM\x5\x1B\xE\x2JM\a&\x2\x2KM\x5\x1D\xF\x2LG\x3\x2\x2\x2LH\x3\x2\x2\x2"+
		"LI\x3\x2\x2\x2LJ\x3\x2\x2\x2LK\x3\x2\x2\x2MP\x3\x2\x2\x2NL\x3\x2\x2\x2"+
		"NO\x3\x2\x2\x2OQ\x3\x2\x2\x2PN\x3\x2\x2\x2QR\x6\x5\x4\x2R\n\x3\x2\x2\x2"+
		"SW\a$\x2\x2TX\n\x4\x2\x2UV\a$\x2\x2VX\a$\x2\x2WT\x3\x2\x2\x2WU\x3\x2\x2"+
		"\x2XY\x3\x2\x2\x2YW\x3\x2\x2\x2YZ\x3\x2\x2\x2Z[\x3\x2\x2\x2[\\\a$\x2\x2"+
		"\\]\x6\x6\x5\x2]\f\x3\x2\x2\x2^\x61\t\x5\x2\x2_\x61\x5\x1D\xF\x2`^\x3"+
		"\x2\x2\x2`_\x3\x2\x2\x2\x61\x62\x3\x2\x2\x2\x62`\x3\x2\x2\x2\x62\x63\x3"+
		"\x2\x2\x2\x63\x64\x3\x2\x2\x2\x64\x65\x6\a\x6\x2\x65\xE\x3\x2\x2\x2\x66"+
		"j\a\x62\x2\x2gk\n\x6\x2\x2hi\a\x62\x2\x2ik\a\x62\x2\x2jg\x3\x2\x2\x2j"+
		"h\x3\x2\x2\x2kl\x3\x2\x2\x2lj\x3\x2\x2\x2lm\x3\x2\x2\x2mn\x3\x2\x2\x2"+
		"no\a\x62\x2\x2op\x6\b\a\x2p\x10\x3\x2\x2\x2qu\a$\x2\x2rv\n\x4\x2\x2st"+
		"\a$\x2\x2tv\a$\x2\x2ur\x3\x2\x2\x2us\x3\x2\x2\x2vw\x3\x2\x2\x2wu\x3\x2"+
		"\x2\x2wx\x3\x2\x2\x2xy\x3\x2\x2\x2yz\a$\x2\x2z{\x6\t\b\x2{\x12\x3\x2\x2"+
		"\x2|\x80\x5\x19\r\x2}\x80\a\x61\x2\x2~\x80\x5\x1D\xF\x2\x7F|\x3\x2\x2"+
		"\x2\x7F}\x3\x2\x2\x2\x7F~\x3\x2\x2\x2\x80\x84\x3\x2\x2\x2\x81\x83\n\a"+
		"\x2\x2\x82\x81\x3\x2\x2\x2\x83\x86\x3\x2\x2\x2\x84\x82\x3\x2\x2\x2\x84"+
		"\x85\x3\x2\x2\x2\x85\x87\x3\x2\x2\x2\x86\x84\x3\x2\x2\x2\x87\x88\x6\n"+
		"\t\x2\x88\x14\x3\x2\x2\x2\x89\x8A\t\b\x2\x2\x8A\x8B\x3\x2\x2\x2\x8B\x8C"+
		"\b\v\x2\x2\x8C\x16\x3\x2\x2\x2\x8D\x8F\a\xF\x2\x2\x8E\x8D\x3\x2\x2\x2"+
		"\x8E\x8F\x3\x2\x2\x2\x8F\x90\x3\x2\x2\x2\x90\x91\a\f\x2\x2\x91\x18\x3"+
		"\x2\x2\x2\x92\x93\t\t\x2\x2\x93\x1A\x3\x2\x2\x2\x94\x95\t\n\x2\x2\x95"+
		"\x1C\x3\x2\x2\x2\x96\x97\a,\x2\x2\x97\x1E\x3\x2\x2\x2\x98\x99\a/\x2\x2"+
		"\x99 \x3\x2\x2\x2\x9A\x9E\a<\x2\x2\x9B\x9D\x5\x15\v\x2\x9C\x9B\x3\x2\x2"+
		"\x2\x9D\xA0\x3\x2\x2\x2\x9E\x9C\x3\x2\x2\x2\x9E\x9F\x3\x2\x2\x2\x9F\xA4"+
		"\x3\x2\x2\x2\xA0\x9E\x3\x2\x2\x2\xA1\xA5\x5#\x12\x2\xA2\xA5\n\v\x2\x2"+
		"\xA3\xA5\a^\x2\x2\xA4\xA1\x3\x2\x2\x2\xA4\xA2\x3\x2\x2\x2\xA4\xA3\x3\x2"+
		"\x2\x2\xA5\xA6\x3\x2\x2\x2\xA6\xA4\x3\x2\x2\x2\xA6\xA7\x3\x2\x2\x2\xA7"+
		"\"\x3\x2\x2\x2\xA8\xA9\a^\x2\x2\xA9\xAA\x5\x17\f\x2\xAA$\x3\x2\x2\x2\x19"+
		"\x2*\x30\x32;=\x45LNWY`\x62jluw\x7F\x84\x8E\x9E\xA4\xA6\x3\b\x2\x2";
	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}
} // namespace dbsc.Core.Antlr

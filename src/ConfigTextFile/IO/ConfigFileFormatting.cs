namespace ConfigTextFile.IO
{
	using System.Collections.Generic;

	/// <summary>
	/// Describes how to format a <see cref="ConfigFile"/> when writing it as text.
	/// </summary>
	public sealed class ConfigFileFormatting
	{
		/// <summary>
		/// Default formatting. Same as using the parameterless constuctor.
		/// </summary>
		public static readonly ConfigFileFormatting Default = new();
		private readonly char[] quotesInOrderOfPreference;
		private QuotePreference quotePreference;
		/// <summary>
		/// Creates a new instance with default values
		/// </summary>
		public ConfigFileFormatting()
		{
			Indentation = "\t";
			ExtraBlankLines = new Dictionary<ConfigFileToken, ConfigFileToken>()
			{
				{ ConfigFileToken.Value, ConfigFileToken.Key | ConfigFileToken.Comment },
				{ ConfigFileToken.EndArray, ConfigFileToken.Key | ConfigFileToken.Comment },
				{ ConfigFileToken.EndSection, ConfigFileToken.Key | ConfigFileToken.Comment }
			};
			SectionBraceSameLine = true;
			WriteEqualsBeforeArray = true;
			AlwaysQuoteKeys = false;
			AlwaysQuoteValues = false;
			quotesInOrderOfPreference = new char[3];
			QuotePreference = QuotePreference.Double_Single_Backtick;
		}
		/// <summary>
		/// The string to use for 1 level of indentation.
		/// (Default: 1 tab per 1 level of indentation)
		/// </summary>
		public string Indentation { get; set; }
		/// <summary>
		/// Defines which elements have extra blank lines.
		/// The Key is a single flag, it means what element should have blank lines written after it.
		/// The Value is a set of flags, it defines which next elements trigger an extra blank line.
		/// e.g. Key = ConfigFileToken.Value, Value = ConfigFileToken.EndSection | ConfigFileToken.Key. This means, after writing a Value, an extra blank line is appended if the next element is either EndSection or a Key.
		/// By default, extra blank lines are written between these elements...
		/// After Value, if next token is Key or Comment.
		/// After EndArray, if the next token is Key or Comment.
		/// After EndSection, if the next token is Key or Comment.
		/// </summary>
		public IDictionary<ConfigFileToken, ConfigFileToken> ExtraBlankLines { get; set; }
		/// <summary>
		/// If true, Section keys will be written on the same line as the opening section brace.
		/// If false, Section keys will have a line break before the opening section brace.
		/// (Default: true)
		/// </summary>
		public bool SectionBraceSameLine { get; set; }
		/// <summary>
		/// If true, Arrays will be written like this: Key=[values].
		/// If false, Arrays will be written like this: Key[values].
		/// (Default: true)
		/// </summary>
		public bool WriteEqualsBeforeArray { get; set; }
		/// <summary>
		/// If true, keys will always be quoted.
		/// If false, keys will only be quoted if they span multiple lines, or start with characters which
		/// would make them syntactically invalid if they were not quoted, or if they start with whitespace.
		/// (Default: false)
		/// </summary>
		public bool AlwaysQuoteKeys { get; set; }
		/// <summary>
		/// If true, single values and Array values will always be quoted.
		/// If false, single values and Array values will only be quoted if they span multiple lines, or start with characters which
		/// would make them syntactically invalid if they were not quoted, or if they start with whitespace.
		/// (Default: false)
		/// </summary>
		public bool AlwaysQuoteValues { get; set; }
		/// <summary>
		/// The order in which quotes are preferred when strings need to be quoted.
		/// If strings contain quotes, will use the first preference it can.
		/// (Default: Double_Single_Backtick)
		/// </summary>
		public QuotePreference QuotePreference
		{
			get => quotePreference;
			set
			{
				quotePreference = value;
				switch (value)
				{
					case QuotePreference.Double_Single_Backtick:
						quotesInOrderOfPreference[0] = '"';
						quotesInOrderOfPreference[1] = '\'';
						quotesInOrderOfPreference[2] = '`';
						break;
					case QuotePreference.Double_Backtick_Single:
						quotesInOrderOfPreference[0] = '"';
						quotesInOrderOfPreference[1] = '`';
						quotesInOrderOfPreference[2] = '\'';
						break;
					case QuotePreference.Single_Double_Backtick:
						quotesInOrderOfPreference[0] = '\'';
						quotesInOrderOfPreference[1] = '"';
						quotesInOrderOfPreference[2] = '`';
						break;
					case QuotePreference.Single_Backtick_Double:
						quotesInOrderOfPreference[0] = '\'';
						quotesInOrderOfPreference[1] = '`';
						quotesInOrderOfPreference[2] = '"';
						break;
					case QuotePreference.Backtick_Double_Single:
						quotesInOrderOfPreference[0] = '`';
						quotesInOrderOfPreference[1] = '"';
						quotesInOrderOfPreference[2] = '\'';
						break;
					case QuotePreference.Backtick_Single_Double:
						quotesInOrderOfPreference[0] = '`';
						quotesInOrderOfPreference[1] = '\'';
						quotesInOrderOfPreference[2] = '"';
						break;
				}
			}
		}
		/// <summary>
		/// The quotes in order of preference. Set the property QuotePreference to change this.
		/// You shouldn't need to use this yourself though.
		/// </summary>
		public IReadOnlyList<char> QuotesInOrderOfPreference => quotesInOrderOfPreference;
	}
}

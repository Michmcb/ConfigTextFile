namespace ConfigTextFile
{
	/// <summary>
	/// These are the syntax characters used in config text files.
	/// </summary>
	public static class SyntaxCharacters
	{
		/// <summary>
		/// Delimits individual elements in an array
		/// </summary>
		public const char ArrayElementDelimiter = ',';
		/// <summary>
		/// Denotes the start of an array
		/// </summary>
		public const char ArrayStart = '[';
		/// <summary>
		/// Denotes the end of an array
		/// </summary>
		public const char ArrayEnd = ']';
		/// <summary>
		/// Denotes the start of a section
		/// </summary>
		public const char SectionStart = '{';
		/// <summary>
		/// Denotes the end of a section
		/// </summary>
		public const char SectionEnd = '}';
		/// <summary>
		/// Goes between a key/value, or key/array
		/// </summary>
		public const char ValueStart = '=';
		/// <summary>
		/// Lines starting with this are a comment
		/// </summary>
		public const char CommentStart = '#';
		

		/// <summary>
		/// ArrayElementDelimiter and ArrayEnd
		/// </summary>
		internal static readonly char[] ArrayElementDelimiterAndEnd = new char[] { ArrayElementDelimiter, ArrayEnd };
		/// <summary>
		/// ValueStart, ArrayStart, ScopeStart, \r and \n
		/// </summary>
		internal static readonly char[] EndOfKey = new char[] { ValueStart, ArrayStart, SectionStart, '\r', '\n' };
		/// <summary>
		/// \r and \n
		/// </summary>
		internal static readonly char[] EndOfLine = new char[] { '\r', '\n' };
	}
}

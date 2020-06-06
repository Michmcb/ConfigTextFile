namespace ConfigTextFile.IO
{
	public sealed class SyntaxCharacters
	{
		public const char ArrayElementDelimiter = ',';
		public const char ArrayStart = '[';
		public const char ArrayEnd = ']';
		public const char SectionStart = '{';
		public const char SectionEnd = '}';
		public const char ValueStart = '=';
		public const char CommentStart = '#';
		public const string KeyDelimiter = ":";

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

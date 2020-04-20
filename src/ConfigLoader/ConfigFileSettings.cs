namespace ConfigTextFile
{
	public class ConfigFileSettings
	{
		/// <summary>
		/// Creates settings. All parameters are optional with conventional defaults.
		/// </summary>
		/// <param name="scopeDelimiter">The string placed between nested scope names</param>
		/// <param name="commentChar">The character which, if a line starts with this (ignoring whitespace), denotes a comment</param>
		public ConfigFileSettings(string scopeDelimiter = ":", char commentChar = '#')
		{
			ScopeDelimiter = scopeDelimiter;
			CommentChar = commentChar;
		}
		/// <summary>
		/// The string placed between nested scope names.
		/// </summary>
		public string ScopeDelimiter { get; }
		/// <summary>
		/// The character which, if a line starts with this (ignoring whitespace), denotes a comment
		/// </summary>
		public char CommentChar { get; }
	}
}

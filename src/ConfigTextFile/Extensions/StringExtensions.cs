
namespace ConfigTextFile.Extensions
{
	/// <summary>
	/// Provides extension methods for strings, primarily used to parse ConfigTextFiles.
	/// Note that if the search runs to the end of the string or if the thing in question wasn't found,
	/// it returns String.Length instead of -1.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Returns the index of the first non-whitespace character found, or <paramref name="s"/>.Length, whichever is smaller.
		/// Returns String.Length if the end of string is reached or if <paramref name="startIndex"/> is >= String.Length.
		/// </summary>
		/// <param name="s">The string</param>
		/// <param name="startIndex">The index to start from</param>
		public static int SkipWhiteSpace(this string s, int startIndex)
		{
			if (startIndex >= s.Length)
			{
				return s.Length;
			}
			int i = startIndex;
			for (; i < s.Length && char.IsWhiteSpace(s[i]); i++) { }
			return i;
		}
		/// <summary>
		/// Skips all whitespace and any comments.
		/// Returns the index of the first non-whitespace non-comment character.
		/// Returns String.Length if the end of string is reached or if <paramref name="startIndex"/> is >= String.Length.
		/// </summary>
		/// <param name="s">The string</param>
		/// <param name="startIndex">The index to start from</param>
		/// <param name="commentChar">The char that denotes a comment</param>
		public static int SkipWhiteSpaceAndComments(this string s, int startIndex, char commentChar)
		{
			if (startIndex >= s.Length)
			{
				return s.Length;
			}
			int i = startIndex;
			while (true)
			{
				for (; i < s.Length && char.IsWhiteSpace(s[i]); i++) { }
				if (i < s.Length && s[i] == commentChar)
				{
					i = IndexOfNewLine(s, i);
				}
				else
				{
					break;
				}
			}
			return i;
		}
		/// <summary>
		/// Returns the index of the first \n or \r character found, or <paramref name="s"/>.Length, whichever is smaller.
		/// Returns String.Length if the end of string is reached or if <paramref name="startIndex"/> is >= String.Length.
		/// </summary>
		/// <param name="s">The string</param>
		/// <param name="startIndex">The index to start from</param>
		public static int IndexOfNewLine(this string s, int startIndex)
		{
			if (startIndex >= s.Length)
			{
				return s.Length;
			}
			int i = s.IndexOfAny(new[] { '\n', '\r' }, startIndex);
			return i == -1 ? s.Length : i;
		}
		/// <summary>
		/// Like String.IndexOf, but returns String.Length if the end of string is reached or if <paramref name="startIndex"/> is >= String.Length.
		/// </summary>
		/// <param name="s">The string</param>
		/// <param name="startIndex">The index to start from</param>
		public static int IndexOfSafe(this string s, char value, int startIndex)
		{
			if (startIndex >= s.Length)
			{
				return s.Length;
			}
			int i = s.IndexOf(value, startIndex);
			return i == -1 ? s.Length : i;
		}
		/// <summary>
		/// Like String.IndexOfAny, but returns String.Length if the end of string is reached or if <paramref name="startIndex"/> is >= String.Length.
		/// </summary>
		/// <param name="s">The string</param>
		/// <param name="startIndex">The index to start from</param>
		public static int IndexOfAnySafe(this string s, char[] anyOf, int startIndex)
		{
			if (startIndex >= s.Length)
			{
				return s.Length;
			}
			int i = s.IndexOfAny(anyOf, startIndex);
			return i == -1 ? s.Length : i;
		}
	}
}

namespace ConfigTextFile.IO
{
	using System.Linq;
	/// <summary>
	/// Some helper methods
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Returns the zero-based index of the first occurrence in this instance of a character NOT in <paramref name="chars"/>, starting from <paramref name="startIndex"/>.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="chars"></param>
		/// <param name="startIndex"></param>
		public static int IndexOfNotAny(this string str, char[] chars, int startIndex)
		{
			int i = startIndex;
			for (; i < str.Length; i++)
			{
				if (!chars.Contains(str[i]))
				{
					return i;
				}
			}
			return -1;
		}
	}
}

using System;
using System.IO;

namespace ConfigTextFile.IO
{
	public static class ExtensionsAndHelpers
	{
		public static void WriteLine(this StreamWriter writer, in ReadOnlySpan<char> str)
		{
			foreach (char c in str)
			{
				writer.Write(c);
			}
			writer.WriteLine();
		}
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
				if (!chars.ArrayContains(str[i]))
				{
					return i;
				}
			}
			return -1;
		}
		/// <summary>
		/// Returns true if the array contains <paramref name="c"/>
		/// </summary>
		public static bool ArrayContains(this char[] array, char c)
		{
			foreach (char ac in array)
			{
				if (ac == c)
				{
					return true;
				}
			}
			return false;
		}
	}
}

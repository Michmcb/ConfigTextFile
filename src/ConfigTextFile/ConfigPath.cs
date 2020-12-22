namespace ConfigTextFile
{
	using System;

	/// <summary>
	/// Helps with paths, broken up by <see cref="Separator"/>.
	/// </summary>
	public static class ConfigPath
	{
		/// <summary>
		/// Delimits paths names. e.g. Section1:Section2:Key
		/// </summary>
		public const char Separator = ':';
		/// <summary>
		/// Joins <paramref name="path"/> and <paramref name="key"/>, making sure that they are broken up with <see cref="Separator"/>.
		/// If <paramref name="path"/> is null or empty, just returns <paramref name="key"/>.
		/// </summary>
		/// <param name="path">The parent path.</param>
		/// <param name="key">The child key. Should not contain any instances of <see cref="Separator"/></param>
		/// <returns>A joined path.</returns>
		public static string Join(string? path, string key)
		{
			return
				!key.Contains(Separator)
				? !string.IsNullOrEmpty(path)
					? path[path.Length - 1] == Separator
						? path + key // If path ends with a separator, just concat them normally
						: string.Concat(path, Separator, key) // If it doesn't we have to insert a separator
					: key // Path being null/empty, just return the key
				: throw new ArgumentException("key should not contain a section delimiter (which is " + Separator + ")", nameof(key)); // Contains separator is bad
		}
	}
}

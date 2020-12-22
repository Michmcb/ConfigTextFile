namespace ConfigTextFile
{
	using ConfigTextFile.IO;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	/// <summary>
	/// A single config text file
	/// </summary>
	public sealed class ConfigFile
	{
		/// <summary>
		/// Creates a new instance with <see cref="Root"/>
		/// </summary>
		public ConfigFile(IEqualityComparer<string> keyComparer)
		{
			Root = new ConfigSectionElement(keyComparer);
		}
		/// <summary>
		/// Creates a new instance. <paramref name="root"/> must be created by using the parameterless constructor of <see cref="ConfigSectionElement"/>.
		/// </summary>
		public ConfigFile(ConfigSectionElement root)
		{
			if (root.Key?.Length != 0)
			{
				throw new ArgumentException("The Key of root must be an empty string to use it as the root section", nameof(root));
			}
			if (root.Path?.Length != 0)
			{
				throw new ArgumentException("The Path of root must be an empty string to use it as the root section", nameof(root));
			}
			Root = root;
		}
		/// <summary>
		/// The root-level <see cref="ConfigSectionElement"/>. Its Key/Path are empty strings.
		/// Note that because this represents the highest level it cannot have comments preceding it (those would be applied to the first element in the file instead)
		/// </summary>
		public ConfigSectionElement Root { get; }
		/// <summary>
		/// Calls <see cref="ConfigFileWriter.WriteSection(ConfigSectionElement)"/>, passing <see cref="Root"/>.
		/// </summary>
		/// <param name="stream">The stream to save the file to.</param>
		public void Save(ConfigFileWriter stream)
		{
			stream.WriteSection(Root);
		}
		/// <summary>
		/// Creates a new Dictionary filled with the paths and values of every <see cref="ConfigStringElement"/> and <see cref="ConfigArrayElement"/>.
		/// </summary>
		public Dictionary<string, string> CreateStringDictionary()
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();
			FillStringDictionary(dict);
			return dict;
		}
		/// <summary>
		/// Fills <paramref name="dict"/> with the paths and values of every <see cref="ConfigStringElement"/> and <see cref="ConfigArrayElement"/>.
		/// If <paramref name="overwrite"/> is true, keys already present are overwritten. Otherwise, a <see cref="ArgumentException"/> is thrown.
		/// </summary>
		/// <param name="dict">The dictionary to fill.</param>
		/// <param name="overwrite">If true, overwrites key/value pairs in the provided <paramref name="dict"/>. Otherwise, throws an exception.</param>
		public void FillStringDictionary(IDictionary<string, string> dict, bool overwrite = true)
		{
			Stack<ConfigSectionElement> sections = new Stack<ConfigSectionElement>();
			sections.Push(Root);
			while (sections.Count > 0)
			{
				ConfigSectionElement s = sections.Pop();
				foreach (IConfigElement item in s.Elements.Values)
				{
					string str = item.Value;
					Debug.Assert(item.Type != ConfigElementType.Invalid, "Should never get an invalid element when iterating elements");
					switch (item.Type)
					{
						case ConfigElementType.String:
							if (overwrite)
							{
								dict[item.Path] = item.Value;
							}
							else
							{
								dict.Add(item.Path, item.Value);
							}
							break;
						case ConfigElementType.Array:
							foreach (ConfigStringElement arrayItem in item.AsArrayElement().Elements)
							{
								if (overwrite)
								{
									dict[arrayItem.Path] = arrayItem.Value;
								}
								else
								{
									dict.Add(arrayItem.Path, arrayItem.Value);
								}
							}
							break;
						case ConfigElementType.Section:
							sections.Push(item.AsSectionElement());
							break;
					}
				}
			}
		}
		/// <summary>
		/// Creates a new Dictionary filled with the paths and values of every <see cref="ConfigStringElement"/> and <see cref="ConfigArrayElement"/>, from all <paramref name="configs"/>.
		/// If <paramref name="overwrite"/> is true, keys already present are overwritten. Otherwise, a <see cref="ArgumentException"/> is thrown.
		/// </summary>
		/// <param name="overwrite">If true, overwrites key/value pairs in the provided dict. Otherwise, throws an exception.</param>
		/// <param name="configs">The ConfigFiles from which to create a Dictionary.</param>
		public static Dictionary<string, string> CreateStringDictionary(bool overwrite, params ConfigFile[] configs)
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();
			FillStringDictionary(dict, overwrite, configs);
			return dict;
		}
		/// <summary>
		/// Fills <paramref name="dict"/> with the paths and values of every <see cref="ConfigStringElement"/> and <see cref="ConfigArrayElement"/>, from all <paramref name="configs"/>.
		/// If <paramref name="overwrite"/> is true, keys already present are overwritten. Otherwise, a <see cref="System.ArgumentException"/> is thrown.
		/// </summary>
		/// <param name="dict">The dictionary to fill.</param>
		/// <param name="overwrite">If true, overwrites key/value pairs in the provided dict. Otherwise, throws an exception.</param>
		/// <param name="configs">The ConfigFiles to use to fill the dictionary.</param>
		public static void FillStringDictionary(IDictionary<string, string> dict, bool overwrite, params ConfigFile[] configs)
		{
			foreach (ConfigFile config in configs)
			{
				config.FillStringDictionary(dict, overwrite);
			}
		}
		/// <summary>
		/// Attempts to load the file located at <paramref name="path"/>, interpreted using <paramref name="encoding"/>.
		/// If it cannot be loaded, throws a <see cref="ConfigFileFormatException"/>.
		/// </summary>
		/// <param name="path">Path of the file to load.</param>
		/// <param name="encoding">Encoding to use.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <param name="keyComparer">The comparer to use when comparing keys for elements added to a <see cref="ConfigSectionElement"/>. If null, defaults to <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The loaded <see cref="ConfigFile"/></returns>
		public static ConfigFile LoadFile(string path, Encoding encoding, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load, IEqualityComparer<string>? keyComparer = null)
		{
			using StreamReader sin = new StreamReader(path, encoding);
			return LoadFile(sin, commentLoading, keyComparer);
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// If it cannot be loaded, throws a <see cref="ConfigFileFormatException"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <param name="keyComparer">The comparer to use when comparing keys for elements added to a <see cref="ConfigSectionElement"/>. If null, defaults to <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The loaded <see cref="ConfigFile"/></returns>
		public static ConfigFile LoadFile(StreamReader stream, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load, IEqualityComparer<string>? keyComparer = null)
		{
			return LoadFile(new ConfigFileReader(stream, false), commentLoading, keyComparer);
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// If it cannot be loaded, throws a <see cref="ConfigFileFormatException"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <param name="keyComparer">The comparer to use when comparing keys for elements added to a <see cref="ConfigSectionElement"/>. If null, defaults to <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The loaded <see cref="ConfigFile"/></returns>
		public static ConfigFile LoadFile(ConfigFileReader stream, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load, IEqualityComparer<string>? keyComparer = null)
		{
			LoadResult result = TryLoadFile(stream, commentLoading, keyComparer);
			return result.ConfigTextFile ?? throw new ConfigFileFormatException(result.ErrMsg);
		}
		/// <summary>
		/// Attempts to load the file located at <paramref name="path"/>, interpreted using <paramref name="encoding"/>.
		/// </summary>
		/// <param name="path">Path of the file to load.</param>
		/// <param name="encoding">Encoding to use.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <param name="keyComparer">The comparer to use when comparing keys for elements added to a <see cref="ConfigSectionElement"/>. If null, defaults to <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>A <see cref="LoadResult"/> which indicates success/failure.</returns>
		public static LoadResult TryLoadFile(string path, Encoding encoding, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load, IEqualityComparer<string>? keyComparer = null)
		{
			using StreamReader sin = new StreamReader(path, encoding);
			return TryLoadFile(sin, commentLoading, keyComparer);
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <param name="keyComparer">The comparer to use when comparing keys for elements added to a <see cref="ConfigSectionElement"/>. If null, defaults to <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>A <see cref="LoadResult"/> which indicates success/failure.</returns>
		public static LoadResult TryLoadFile(StreamReader stream, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load, IEqualityComparer<string>? keyComparer = null)
		{
			return TryLoadFile(new ConfigFileReader(stream, false), commentLoading, keyComparer);
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <param name="keyComparer">The comparer to use when comparing keys for elements added to a <see cref="ConfigSectionElement"/>. If null, defaults to <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>A <see cref="LoadResult"/> which indicates success/failure.</returns>
		public static LoadResult TryLoadFile(ConfigFileReader stream, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load, IEqualityComparer<string>? keyComparer = null)
		{
			// If we are ignoring comments then we'll assign everything an empty array.
			// Otherwise, we'll need lists.
			ICollection<string> comments = commentLoading == LoadCommentsPreference.Ignore
				? (ICollection<string>)Array.Empty<string>()
				: new List<string>();
			Stack<ConfigSectionElement> parentSections = new Stack<ConfigSectionElement>();
			// Default to ordinal comparison
			keyComparer ??= StringComparer.Ordinal;
			// Root can never have comments
			ConfigSectionElement root = new ConfigSectionElement(keyComparer);
			ConfigSectionElement currentSection = root;
			string key = string.Empty;
			try
			{
				while (stream.MoreToRead)
				{
					ReadCfgToken fRead = stream.Read();
					switch (fRead.Type)
					{
						case ConfigFileToken.Key:
							key = fRead.Value;
							break;
						case ConfigFileToken.Value:
							ConfigStringElement singleString = new ConfigStringElement(key, fRead.Value, comments, copyComments: false);
							if (!currentSection.TryAddElement(singleString))
							{
								return new LoadResult(string.Concat("Duplicate key \"", ConfigPath.Join(currentSection.Path, key), "\" was found"));
							}
							if (commentLoading != LoadCommentsPreference.Ignore)
							{
								comments = new List<string>();
							}
							break;
						case ConfigFileToken.StartArray:
							ConfigArrayElement array = new ConfigArrayElement(key, Array.Empty<string>(), comments, copyComments: false);
							if (!currentSection.TryAddElement(array))
							{
								return new LoadResult(string.Concat("Duplicate key \"", ConfigPath.Join(currentSection.Path, key), "\" was found"));
							}
							if (commentLoading != LoadCommentsPreference.Ignore)
							{
								comments = new List<string>();
							}
							while (true)
							{
								fRead = stream.Read();
								if (fRead.Type == ConfigFileToken.ArrayValue)
								{
									// TODO it would be nice to allow comments inside arrays, though this does present some awkwardness.
									array.AddNewString(fRead.Value);
								}
								else
								{
									break;
								}
							}
							break;
						case ConfigFileToken.StartSection:
							parentSections.Push(currentSection);
							ConfigSectionElement newSection = new ConfigSectionElement(key, keyComparer, comments, copyComments: false);
							if (!currentSection.TryAddElement(newSection))
							{
								return new LoadResult(string.Concat("Duplicate key \"", ConfigPath.Join(currentSection.Path, key), "\" was found"));
							}
							if (commentLoading != LoadCommentsPreference.Ignore)
							{
								comments = new List<string>();
							}
							currentSection = newSection;
							break;
						case ConfigFileToken.EndSection:
							// Section end, pop back to the upper scope
							currentSection = parentSections.Pop();
							break;
						case ConfigFileToken.Comment:
							if (commentLoading == LoadCommentsPreference.Load)
							{
								comments.Add(fRead.Value);
							}
							break;
						case ConfigFileToken.Finish:
						case ConfigFileToken.EndArray:
						default:
							break;
					}
				}
				return new LoadResult(new ConfigFile(root));
			}
			catch (ConfigFileFormatException ex)
			{
				return new LoadResult(ex.Message);
			}
		}
	}
}

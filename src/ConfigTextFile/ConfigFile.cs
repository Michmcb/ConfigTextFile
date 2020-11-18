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
		/// Creates a new instance with <see cref="Root"/> being a new instance with its Key/Path set to <see cref="string.Empty"/>.
		/// </summary>
		public ConfigFile()
		{
			Root = new ConfigSectionElement(string.Empty, string.Empty, (ICollection<string>)Array.Empty<string>());
		}
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public ConfigFile(ConfigSectionElement root)
		{
			Root = root;
		}
		/// <summary>
		/// The root-level ConfigSection. Its Key/Path are empty strings, and it is not included in AllElements.
		/// Note that because this represents the highest level it cannot have comments preceding it (those would be applied to the first element in the file instead)
		/// </summary>
		public ConfigSectionElement Root { get; }
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
		/// <returns>The loaded <see cref="ConfigFile"/></returns>
		public static ConfigFile LoadFile(string path, Encoding encoding, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load)
		{
			using StreamReader sin = new StreamReader(path, encoding);
			return LoadFile(sin, commentLoading);
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// If it cannot be loaded, throws a <see cref="ConfigFileFormatException"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <returns>The loaded <see cref="ConfigFile"/></returns>
		public static ConfigFile LoadFile(StreamReader stream, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load)
		{
			return LoadFile(new ConfigFileReader(stream, false), commentLoading);
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// If it cannot be loaded, throws a <see cref="ConfigFileFormatException"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <returns>The loaded <see cref="ConfigFile"/></returns>
		public static ConfigFile LoadFile(ConfigFileReader stream, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load)
		{
			LoadResult result = TryLoadFile(stream, commentLoading);
			return result.ConfigTextFile ?? throw new ConfigFileFormatException(result.ErrMsg);
		}
		/// <summary>
		/// Attempts to load the file located at <paramref name="path"/>, interpreted using <paramref name="encoding"/>.
		/// </summary>
		/// <param name="path">Path of the file to load.</param>
		/// <param name="encoding">Encoding to use.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <returns>A <see cref="LoadResult"/> which indicates success/failure.</returns>
		public static LoadResult TryLoadFile(string path, Encoding encoding, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load)
		{
			using StreamReader sin = new StreamReader(path, encoding);
			return TryLoadFile(sin, commentLoading);
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <returns>A <see cref="LoadResult"/> which indicates success/failure.</returns>
		public static LoadResult TryLoadFile(StreamReader stream, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load)
		{
			return TryLoadFile(new ConfigFileReader(stream, false), commentLoading);
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="commentLoading">Defines how to load comments.</param>
		/// <returns>A <see cref="LoadResult"/> which indicates success/failure.</returns>
		public static LoadResult TryLoadFile(ConfigFileReader stream, LoadCommentsPreference commentLoading = LoadCommentsPreference.Load)
		{
			ICollection<string> comments = commentLoading == LoadCommentsPreference.Load ? new List<string>() : (ICollection<string>)Array.Empty<string>();
			Stack<ConfigSectionElement> parentSections = new Stack<ConfigSectionElement>();
			string currentSectionPath = string.Empty;
			// Root can never have comments
			ConfigSectionElement root = new ConfigSectionElement(string.Empty, string.Empty, (ICollection<string>)Array.Empty<string>());
			ConfigSectionElement currentSection = root;
			HashSet<string> allPaths = new HashSet<string>();
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
							string path = currentSectionPath + key;
							ConfigStringElement singleString = new ConfigStringElement(key, path, fRead.Value, comments);
							if (commentLoading == LoadCommentsPreference.Load)
							{
								comments = new List<string>();
							}
							if (!allPaths.Contains(path))
							{
								allPaths.Add(path);
							}
							else
							{
								return new LoadResult(string.Concat("Duplicate key \"", path, "\" was found"));
							}
							currentSection.Elements.Add(key, singleString);
							break;
						case ConfigFileToken.StartArray:
							string arrayPath = currentSectionPath + key;
							ConfigArrayElement array = new ConfigArrayElement(key, arrayPath, comments);
							if (commentLoading == LoadCommentsPreference.Load)
							{
								comments = new List<string>();
							}
							if (!allPaths.Contains(arrayPath))
							{
								allPaths.Add(arrayPath);
							}
							else
							{
								return new LoadResult(string.Concat("Duplicate key \"", arrayPath, "\" was found "));
							}
							currentSection.Elements.Add(key, array);
							int index = 0;
							while (true)
							{
								fRead = stream.Read();
								if (fRead.Type == ConfigFileToken.ArrayValue)
								{
									string arrayIndex = index.ToString();
									string arrayElementPath = string.Concat(arrayPath, SyntaxCharacters.SectionDelimiter, arrayIndex);
									// TODO it would be nice to allow comments inside arrays
									ConfigStringElement arrayElement = new ConfigStringElement(arrayIndex, arrayElementPath, fRead.Value, (ICollection<string>)Array.Empty<string>());
									if (!allPaths.Contains(arrayElementPath))
									{
										allPaths.Add(arrayElementPath);
									}
									else
									{
										return new LoadResult(string.Concat("Duplicate key \"", arrayElementPath, "\" was found"));
									}
									array.Elements.Add(arrayElement);
									++index;
								}
								else
								{
									break;
								}
							}
							break;
						case ConfigFileToken.StartSection:
							parentSections.Push(currentSection);
							currentSectionPath += key;
							if (!allPaths.Contains(currentSectionPath))
							{
								ConfigSectionElement newSection = new ConfigSectionElement(key, currentSectionPath, comments);
								if (commentLoading == LoadCommentsPreference.Load)
								{
									comments = new List<string>();
								}
								currentSection.Elements.Add(key, newSection);
								currentSection = newSection;
								allPaths.Add(currentSectionPath);
								currentSectionPath += SyntaxCharacters.SectionDelimiter;
							}
							else
							{
								return new LoadResult(string.Concat("Duplicate key \"", currentSectionPath, "\" was found"));
							}
							break;
						case ConfigFileToken.EndSection:
							// Section end, pop back to the upper scope
							currentSection = parentSections.Pop();
							currentSectionPath = currentSection.Path.Length > 0 ? currentSection.Path + SyntaxCharacters.SectionDelimiter : string.Empty;
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

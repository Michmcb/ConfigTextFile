namespace ConfigTextFile
{
	using ConfigTextFile.IO;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;

	/*
https://github.com/dotnet/extensions/blob/master/src/Configuration/Config.NewtonsoftJson/src/NewtonsoftJsonConfigurationSource.cs
https://github.com/dotnet/extensions/blob/master/src/Configuration/Config.NewtonsoftJson/src/NewtonsoftJsonConfigurationProvider.cs
https://github.com/dotnet/extensions/blob/master/src/Configuration/Config.NewtonsoftJson/src/NewtonsoftJsonConfigurationExtensions.cs

In source, all you should do is EnsureDefaults(builder); and return new ConfigTextFileConfigurationProvider(this, FileProvider);
In Provider, it's just ConfigFile file = ConfigFile.LoadFile(new StreamReader(stream)); and file.FillStringDictionary(Data);

For Extensions, make a new ConfigTextFileConfigurationSource, just with FileProvider, Path, Optional, and ReloadOnChange set. Then call ResolveFileProvider() on it, add it to builder, and done.
*/
	/// <summary>
	/// A single config text file
	/// </summary>
	public sealed class ConfigFile
	{
		/// <summary>
		/// Creates a new instance with <see cref="AllElements"/> being a new empty dictionary and
		/// <see cref="Root"/> being a new instance with its Key/Path set to <see cref="string.Empty"/>.
		/// </summary>
		public ConfigFile()
		{
			AllElements = new Dictionary<string, IConfigElement>();
			Root = new ConfigSectionElement(string.Empty, string.Empty);
		}
		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="allElements"></param>
		/// <param name="root"></param>
		public ConfigFile(IDictionary<string, IConfigElement> allElements, ConfigSectionElement root)
		{
			AllElements = allElements;
			Root = root;
		}
		/// <summary>
		/// All IConfigElements, keyed by section:name:arrayindex.
		/// </summary>
		public IDictionary<string, IConfigElement> AllElements { get; }
		/// <summary>
		/// The root-level ConfigSection. Its Key/Path are empty strings, and it is not included in AllElements.
		/// </summary>
		public ConfigSectionElement Root { get; }
		/// <summary>
		/// Creates a Dictionary, with the same keys as <see cref="AllElements"/>, but the values are the string values of all ConfigElements.
		/// </summary>
		public Dictionary<string, string> CreateStringDictionary()
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();
			FillStringDictionary(dict);
			return dict;
		}
		/// <summary>
		/// Fills the provided <paramref name="dict"/> with the same keys as <see cref="AllElements"/>, but the values are the string values of all ConfigElements.
		/// If <paramref name="overwrite"/> is true, keys already present are overwritten. Otherwise, a <see cref="System.ArgumentException"/> is thrown.
		/// </summary>
		/// <param name="dict">The dictionary to fill.</param>
		/// <param name="overwrite">If true, overwrites key/value pairs in the provided dict. Otherwise, throws an exception.</param>
		public void FillStringDictionary(IDictionary<string, string> dict, bool overwrite = true)
		{
			IEnumerable<IConfigElement> strings = AllElements.Values.Where(x => x.Type == ConfigElementType.String);
			if (overwrite)
			{
				foreach (IConfigElement sv in strings)
				{
					dict[sv.Path] = sv.Value;
				}
			}
			else
			{
				foreach (IConfigElement sv in strings)
				{
					dict.Add(sv.Path, sv.Value);
				}
			}
		}
		/// <summary>
		/// Tries to get the <see cref="IConfigElement"/> identified by <paramref name="key"/>.
		/// If it does not exist, returns a <see cref="ConfigInvalidElement"/>.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		public IConfigElement GetElement(string key)
		{
			return AllElements.TryGetValue(key, out IConfigElement section) ? section : ConfigInvalidElement.Inst;
		}
		/// <summary>
		/// Gets or sets a child element's value.
		/// <paramref name="key"/> should refer to a <see cref="ConfigStringElement"/>.
		/// If the key was not found, throws a <see cref="KeyNotFoundException"/>.
		/// If the key was found but it wasn't a <see cref="ConfigStringElement"/>, throws an <see cref="InvalidOperationException"/>.
		/// </summary>
		/// <param name="key">Gets or sets an <see cref="IConfigElement"/>'s Value whose Key property matches this.</param>
		public string this[string key]
		{
			get
			{
				return AllElements.TryGetValue(key, out IConfigElement elem)
					? elem.Value
					: throw new KeyNotFoundException("There is no " + nameof(ConfigStringElement) + " with the key " + key);
			}
			set
			{
				if (AllElements.TryGetValue(key, out IConfigElement elem))
				{
					elem.Value = value;
				}
				else
				{
					throw new KeyNotFoundException("There is no " + nameof(ConfigStringElement) + " with the key " + key);
				}
			}
		}
		/// <summary>
		/// Creates a Dictionary, with the same keys as all of the <see cref="AllElements"/> property of all <paramref name="configs"/>, but the values are the string values of all ConfigElements.
		/// If <paramref name="overwrite"/> is true, keys already present are overwritten. Otherwise, a <see cref="System.ArgumentException"/> is thrown.
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
		/// Fills the provided <paramref name="dict"/> with the same keys as all of the <see cref="AllElements"/> property of all <paramref name="configs"/>, but the values are the string values of all ConfigElements.
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
		/// <param name="ignoreComments">If true, comments are not loaded.</param>
		/// <returns>The loaded <see cref="ConfigFile"/></returns>
		public static ConfigFile LoadFile(string path, Encoding encoding, bool ignoreComments = false)
		{
			using (StreamReader sin = new StreamReader(path, encoding))
			{
				return LoadFile(sin, ignoreComments);
			}
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// If it cannot be loaded, throws a <see cref="ConfigFileFormatException"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="ignoreComments">If true, comments are not loaded.</param>
		/// <returns>The loaded <see cref="ConfigFile"/></returns>
		public static ConfigFile LoadFile(StreamReader stream, bool ignoreComments = false)
		{
			return LoadFile(new ConfigFileReader(stream, false), ignoreComments);
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// If it cannot be loaded, throws a <see cref="ConfigFileFormatException"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="ignoreComments">If true, comments are not loaded.</param>
		/// <returns>The loaded <see cref="ConfigFile"/></returns>
		public static ConfigFile LoadFile(ConfigFileReader stream, bool ignoreComments = false)
		{
			LoadResult result = TryLoadFile(stream, ignoreComments);
			if (result.ConfigTextFile == null)
			{
				throw new ConfigFileFormatException(result.ErrMsg);
			}
			return result.ConfigTextFile;
		}
		/// <summary>
		/// Attempts to load the file located at <paramref name="path"/>, interpreted using <paramref name="encoding"/>.
		/// </summary>
		/// <param name="path">Path of the file to load.</param>
		/// <param name="encoding">Encoding to use.</param>
		/// <param name="ignoreComments">If true, comments are not loaded.</param>
		/// <returns>A <see cref="LoadResult"/> which indicates success/failure.</returns>
		public static LoadResult TryLoadFile(string path, Encoding encoding, bool ignoreComments = false)
		{
			using (StreamReader sin = new StreamReader(path, encoding))
			{
				return TryLoadFile(sin, ignoreComments);
			}
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="ignoreComments">If true, comments are not loaded.</param>
		/// <returns>A <see cref="LoadResult"/> which indicates success/failure.</returns>
		public static LoadResult TryLoadFile(StreamReader stream, bool ignoreComments = false)
		{
			return TryLoadFile(new ConfigFileReader(stream, false), ignoreComments);
		}
		/// <summary>
		/// Attempts to load a file from <paramref name="stream"/>.
		/// </summary>
		/// <param name="stream">Stream from which to read the file. Does not have to be seekable. Not closed.</param>
		/// <param name="ignoreComments">If true, comments are not loaded.</param>
		/// <returns>A <see cref="LoadResult"/> which indicates success/failure.</returns>
		public static LoadResult TryLoadFile(ConfigFileReader stream, bool ignoreComments = false)
		{
			List<string> comments = new List<string>();
			Stack<ConfigSectionElement> parentSections = new Stack<ConfigSectionElement>();
			string currentSectionPath = string.Empty;
			ConfigSectionElement root = new ConfigSectionElement(string.Empty, string.Empty);
			ConfigSectionElement currentSection = root;
			Dictionary<string, IConfigElement> allElements = new Dictionary<string, IConfigElement>();
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
							comments.Clear();
							if (!allElements.ContainsKey(path))
							{
								allElements.Add(path, singleString);
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
							comments.Clear();
							if (!allElements.ContainsKey(arrayPath))
							{
								allElements.Add(arrayPath, array);
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
									ConfigStringElement arrayElement = new ConfigStringElement(arrayIndex, arrayElementPath, fRead.Value);
									if (!allElements.ContainsKey(arrayElementPath))
									{
										allElements.Add(arrayElementPath, arrayElement);
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
							if (!allElements.ContainsKey(currentSectionPath))
							{
								ConfigSectionElement newSection = new ConfigSectionElement(key, currentSectionPath, comments);
								comments.Clear();
								currentSection.Elements.Add(key, newSection);
								currentSection = newSection;
								allElements.Add(currentSectionPath, currentSection);
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
							if (!ignoreComments)
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
				return new LoadResult(new ConfigFile(allElements, root));
			}
			catch (ConfigFileFormatException ex)
			{
				return new LoadResult(ex.Message);
			}
		}
	}
}

namespace ConfigTextFile
{
	using ConfigTextFile.IO;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;

	public sealed class ConfigFile
	{
		private static readonly ConfigFile Empty = new ConfigFile(null, null);
		public ConfigFile()
		{
			AllElements = new Dictionary<string, IConfigElement>();
		}
		public ConfigFile(IDictionary<string, IConfigElement> tokens, ConfigSectionElement root)
		{
			AllElements = tokens;
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
		public IConfigElement GetElement(string key)
		{
			return AllElements.TryGetValue(key, out IConfigElement section) ? section : ConfigInvalidElement.Inst;
		}
		/// <summary>
		/// Retrieves a string, given a key.
		/// If the key does not exist, returns null.
		/// </summary>
		/// <param name="key">The key of the config value</param>
		public string this[string key]
		{
			get
			{
				return AllElements.TryGetValue(key, out IConfigElement section) ? section.Value : null;
			}
			set
			{
				if (AllElements.TryGetValue(key, out IConfigElement section))
				{
					section.Value = value;
				}
				else
				{
					throw new KeyNotFoundException("There is no TextConfigSection with the key " + key);
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
		public static ConfigFile LoadFile(string path, Encoding encoding)
		{
			using (StreamReader sin = new StreamReader(path, encoding))
			{
				return LoadFile(sin);
			}
		}
		public static ConfigFile LoadFile(StreamReader reader)
		{
			return LoadFile(new ConfigFileReader(reader, false));
		}
		private static ConfigFile LoadFile(ConfigFileReader configFileReader)
		{
			LoadResult result = TryLoadFile(configFileReader);
			if (!result.Success)
			{
				throw new ConfigFileFormatException(result.ErrMsg);
			}
			return result.ConfigTextFile;
		}
		public static LoadResult TryLoadFile(string path, Encoding encoding)
		{
			using (StreamReader sin = new StreamReader(path, encoding))
			{
				return TryLoadFile(sin);
			}
		}
		public static LoadResult TryLoadFile(StreamReader reader)
		{
			return TryLoadFile(new ConfigFileReader(reader, false));
		}
		public static LoadResult TryLoadFile(ConfigFileReader reader)
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
				while (reader.MoreToRead)
				{
					ReadCfgToken fRead = reader.Read();
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
								return new LoadResult(false, Empty, string.Concat("Duplicate key \"", path, "\" was found"));
							}
							currentSection?.Elements.Add(key, singleString);
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
								return new LoadResult(false, Empty, string.Concat("Duplicate key \"", arrayPath, "\" was found "));
							}
							currentSection?.Elements.Add(key, array);
							int index = 0;
							while (true)
							{
								fRead = reader.Read();
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
										return new LoadResult(false, Empty, string.Concat("Duplicate key \"", arrayElementPath, "\" was found"));
									}
									array.Elements.Add(arrayIndex, arrayElement);
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
								currentSection?.Elements.Add(key, newSection);
								currentSection = newSection;
								allElements.Add(currentSectionPath, currentSection);
								currentSectionPath += SyntaxCharacters.SectionDelimiter;
							}
							else
							{
								return new LoadResult(false, Empty, string.Concat("Duplicate key \"", currentSectionPath, "\" was found"));
							}
							break;
						case ConfigFileToken.EndSection:
							// Section end, pop back to the upper scope
							currentSection = parentSections.Pop();
							currentSectionPath = currentSection.Path.Length > 0 ? currentSection.Path + SyntaxCharacters.SectionDelimiter : string.Empty;
							break;
						case ConfigFileToken.Comment:
							comments.Add(fRead.Value);
							break;
						case ConfigFileToken.Finish:
						case ConfigFileToken.EndArray:
						default:
							break;
					}
				}
				return new LoadResult(true, new ConfigFile(allElements, root), string.Empty);
			}
			catch (ConfigFileFormatException ex)
			{
				return new LoadResult(false, Empty, ex.Message);
			}
		}
	}
}

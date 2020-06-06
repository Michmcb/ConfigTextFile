using ConfigTextFile.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConfigTextFile
{
	public class ConfigFile : IConfiguration
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
		/// All IConfigElements, keyed by section:name[arrayIndex].
		/// </summary>
		public IDictionary<string, IConfigElement> AllElements { get; }
		/// <summary>
		/// The root-level ConfigSection. Its Key/Path are empty strings, and it is not included in AllElements.
		/// </summary>
		public ConfigSectionElement Root { get; }
		public ConfigurationReloadToken ChangeToken { get; set; }
		public IConfigElement GetElement(string key)
		{
			return AllElements.TryGetValue(key, out IConfigElement section) ? section : ConfigInvalidElement.Inst;
		}
		public IConfigurationSection GetSection(string key)
		{
			return AllElements.TryGetValue(key, out IConfigElement section) ? section : ConfigInvalidElement.Inst;
		}
		public IEnumerable<IConfigurationSection> GetChildren()
		{
			return AllElements.Values;
		}
		public IChangeToken GetReloadToken()
		{
			throw new NotImplementedException("Currently you can't reload this, so Change Tokens are not implemented yet");
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
			string currentSectionPath = "";
			ConfigSectionElement root = new ConfigSectionElement("", "");
			ConfigSectionElement currentSection = root;
			Dictionary<string, IConfigElement> allElements = new Dictionary<string, IConfigElement>();
			string key = "";
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
									string arrayElementPath = string.Concat(arrayPath, SyntaxCharacters.KeyDelimiter, arrayIndex);
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
								currentSectionPath += SyntaxCharacters.KeyDelimiter;
							}
							else
							{
								return new LoadResult(false, Empty, string.Concat("Duplicate key \"", currentSectionPath, "\" was found"));
							}
							break;
						case ConfigFileToken.EndSection:
							// Section end, pop back to the upper scope
							currentSection = parentSections.Pop();
							currentSectionPath = currentSection.Path.Length > 0 ? currentSection.Path + SyntaxCharacters.KeyDelimiter : "";
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
				return new LoadResult(true, new ConfigFile(allElements, root), "");
			}
			catch (ConfigFileFormatException ex)
			{
				return new LoadResult(false, Empty, ex.Message);
			}
		}
	}
}

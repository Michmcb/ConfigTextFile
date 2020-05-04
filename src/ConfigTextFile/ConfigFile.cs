using ConfigTextFile.Extensions;
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
		private static readonly ConfigFile Empty = new ConfigFile(null);
		public const char ArrayElementDelimiter = ',';
		public const char ArrayStart = '[';
		public const char ArrayEnd = ']';
		public const char SectionStart = '{';
		public const char SectionEnd = '}';
		private const char StringStart = '=';
		public const char CommentStart = '#';
		public const string KeyDelimiter = ":";
		/// <summary>
		/// ArrayElementDelimiter and ArrayEnd
		/// </summary>
		private static readonly char[] ArrayElementDelimiterAndEnd = new char[] { ArrayElementDelimiter, ArrayEnd };
		/// <summary>
		/// StringStart, ArrayStart, and ScopeStart
		/// </summary>
		private static readonly char[] StartSeparators = new char[] { StringStart, ArrayStart, SectionStart };
		public ConfigFile()
		{
			Elements = new Dictionary<string, IConfigElement>();
		}
		public ConfigFile(IDictionary<string, IConfigElement> tokens)
		{
			Elements = tokens;
		}
		/// <summary>
		/// All TextConfigElements, keyed by section:name[arrayIndex].
		/// </summary>
		public IDictionary<string, IConfigElement> Elements { get; }
		public ConfigurationReloadToken ChangeToken { get; set; }
		public IConfigurationSection GetSection(string key)
		{
			return Elements.TryGetValue(key, out IConfigElement section) ? section : ConfigInvalidElement.Inst;
		}
		public IEnumerable<IConfigurationSection> GetChildren()
		{
			return Elements.Values;
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
				return Elements.TryGetValue(key, out IConfigElement section) ? section.Value : null;
			}
			set
			{
				if (Elements.TryGetValue(key, out IConfigElement section))
				{
					section.Value = value;
				}
				else
				{
					throw new KeyNotFoundException("There is no TextConfigSection with the key " + key);
				}
			}
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
			// TODO clean this up a bit I guess.
			// TODO if we encounter a duplicate key, such as a key being the same name as a scope or another key, then throw an exception. Should also do that if we that collide with a full path of an array, too.
			string content = reader.ReadToEnd();
			Stack<ConfigSectionElement> parentSections = new Stack<ConfigSectionElement>();
			string currentSectionPath = "";
			ConfigSectionElement currentSection = null;
			Dictionary<string, IConfigElement> allElements = new Dictionary<string, IConfigElement>();
			for (int i = content.SkipWhiteSpaceAndComments(0, CommentStart); i < content.Length; i = content.SkipWhiteSpaceAndComments(i, CommentStart))
			{
				int keyStart = i;
				char c = content[i];
				string key;
				// The names are allowed to be in quotes, so if the first character is a quote, we find the matching end quote.
				// If it isn't a quote, we have to find any of =, [, or {
				switch (c)
				{
					case '"':
					case '\'':
					case '`':
						// Find the matching closing quote
						i = content.IndexOfSafe(c, i + 1);
						// Adjust to omit the first quote
						key = content.Substring(keyStart + 1, i - (keyStart + 1));
						// Then find a separator
						i = content.IndexOfAnySafe(StartSeparators, i);
						break;
					default:
						// Without quotes, the key's name goes until the separator, sans trailing whitespace
						i = content.IndexOfAnySafe(StartSeparators, i);
						key = content.Substring(keyStart, i - keyStart).TrimEnd();
						break;
					case SectionEnd:
						// Scope end, pop back to the upper scope and advance to the next character
						if (parentSections.Count > 0)
						{
							currentSection = parentSections.Pop();
							currentSectionPath = currentSection != null ? currentSection.Path + KeyDelimiter : "";
							++i;
						}
						else
						{
							return new LoadResult(false, Empty, string.Concat("Unexpected ", SectionEnd.ToString(), " found at " + i));
						}
						continue;
				}
				// i now refers to the separator, or content.Length
				if (i == content.Length)
				{
					return new LoadResult(false, Empty, string.Concat("Unexpected end of file after reading key '", key, "', was expecting a new scope or config value"));
				}
				c = content[i];
				// Now we want to find the first non-whitespace character, which will be the start of the string, collection, or scope name
				i = content.SkipWhiteSpace(i + 1);
				if (i == content.Length)
				{
					return new LoadResult(false, Empty, string.Concat("Unexpected end of file after reading key '", key, "', was expecting a new scope or config value"));
				}
				// And now depending upon what the separator we found was, we process the values a bit differently
				switch (c)
				{
					// A single string. If it is in quotes, it can be multi-line
					case StringStart:
						// However, if the next non-whitespace character is the array start character, then instead parse it as an array
						if (content[i] == ArrayStart)
						{
							goto case ArrayStart;
						}
						Opt<string> result = ParseSingleString(content, ref i);
						if (result.Val != null)
						{
							string path = currentSectionPath + key;
							ConfigStringElement singleString = new ConfigStringElement(key, path, result.Val);
							if (!allElements.ContainsKey(path))
							{
								allElements.Add(path, singleString);
							}
							else
							{
								return new LoadResult(false, Empty, string.Concat("Duplicate key \"", path, "\" was found at position ", i.ToString()));
							}
							currentSection?.Elements.Add(key, singleString);
						}
						else
						{
							return new LoadResult(false, Empty, result.ErrMsg);
						}
						break;
					// An array of strings
					case ArrayStart:
						Opt<List<string>> cresult = ParseStringCollection(content, ref i);
						if (cresult.Val != null)
						{
							string arrayPath = currentSectionPath + key;
							ConfigArrayElement array = new ConfigArrayElement(key, arrayPath);
							if (!allElements.ContainsKey(arrayPath))
							{
								allElements.Add(arrayPath, array);
							}
							else
							{
								return new LoadResult(false, Empty, string.Concat("Duplicate key \"", arrayPath, "\" was found at position ", i.ToString()));
							}
							currentSection?.Elements.Add(key, array);
							int index = 0;
							foreach (string arrayValue in cresult.Val)
							{
								string arrayIndex = index.ToString();
								string arrayElementPath = string.Concat(arrayPath, KeyDelimiter, arrayIndex);
								ConfigStringElement arrayElement = new ConfigStringElement(arrayIndex, arrayElementPath, arrayValue);
								if (!allElements.ContainsKey(arrayElementPath))
								{
									allElements.Add(arrayElementPath, arrayElement);
								}
								else
								{
									return new LoadResult(false, Empty, string.Concat("Duplicate key \"", arrayElementPath, "\" was found at position ", i.ToString()));
								}
								array.Elements.Add(arrayIndex, arrayElement);
								++index;
							}
						}
						else
						{
							return new LoadResult(false, Empty, cresult.ErrMsg);
						}
						break;
					// Scope change
					case SectionStart:
						parentSections.Push(currentSection);
						currentSectionPath += key;
						if (!allElements.ContainsKey(currentSectionPath))
						{
							ConfigSectionElement newSection = new ConfigSectionElement(key, currentSectionPath);
							currentSection?.Elements.Add(key, newSection);
							currentSection = newSection;
							allElements.Add(currentSectionPath, currentSection);
							currentSectionPath += KeyDelimiter;
						}
						else
						{
							return new LoadResult(false, Empty, string.Concat("Duplicate key \"", currentSectionPath, "\" was found at position ", i.ToString()));
						}
						break;
					// This should never happen since we explicitly search for the above 3 chars
					default:
						return new LoadResult(false, Empty, string.Concat("Found unexpected character ", c.ToString(), " when searching for one of '", StringStart.ToString(), ArrayStart.ToString(), SectionStart.ToString(), "'"));
				}
				i = content.SkipWhiteSpace(i);
			}
			if (parentSections.Count == 0)
			{
				return new LoadResult(true, new ConfigFile(allElements), "");
			}
			else
			{
				return new LoadResult(false, Empty, string.Concat(parentSections.Count.ToString(), " Unterminated scope(s), the current scope is ", currentSectionPath, ". Make sure you are terminating scopes with }"));
			}
		}
		private static Opt<string> ParseSingleString(string s, ref int i)
		{
			char c = s[i];
			// If the string starts with any of these quote characters, we need to read until we find a matching end quote character
			if (c == '"' || c == '\'' || c == '`')
			{
				int from = i + 1;
				i = s.IndexOfSafe(c, i + 1);
				// Note the i++; we set it to 1 after the closing quote
				return new Opt<string>(s.Substring(from, i++ - from));
			}
			else
			{
				// Otherwise we read until the end of the line and trim off any excess trailing whitespace
				int from = i;
				i = s.IndexOfNewLine(i);
				return new Opt<string>(s.Substring(from, i - from).TrimEnd());
			}
		}
		private static Opt<List<string>> ParseStringCollection(string s, ref int i)
		{
			List<string> vals = new List<string>();
			while (true)
			{
				char c = s[i];
				// So first we need to parse in the string. That may or may not be surrounded with quotes.
				if (c == '"' || c == '\'' || c == '`')
				{
					int from = i + 1;
					i = s.IndexOfSafe(c, from);
					if (i == s.Length)
					{
						return new Opt<List<string>>(null, string.Concat("Unexpected end of Collection. Was expecting the closing quote '", c.ToString(), "' but found end of string"));
					}
					vals.Add(s.Substring(from, i - from));
				}
				else
				{
					int from = i;
					i = s.IndexOfAny(ArrayElementDelimiterAndEnd, i);
					if (i == -1)
					{
						return new Opt<List<string>>(null, string.Concat("Unexpected end of Collection. Was expecting one of '", ArrayElementDelimiter.ToString(), ArrayEnd.ToString(), "' but found end of string"));
					}
					vals.Add(s.Substring(from, i - from).TrimEnd());
				}
				// Now, we need to find the first instance of an end delimiter, which is either a comma or endDelimiter.
				// This could be the current character we're on so passing just i is correct
				i = s.IndexOfAny(ArrayElementDelimiterAndEnd, i);
				if (i == -1)
				{
					return new Opt<List<string>>(null, string.Concat("Unexpected end of Collection. Was expecting one of '", ArrayElementDelimiter.ToString(), ArrayEnd.ToString(), "' but found end of string"));
				}
				c = s[i];
				switch (c)
				{
					case ArrayElementDelimiter:
						// Now find the first non-whitespace character to read the next one. We might wind up on the end array delimiter if they did this: , ]
						// But that's okay, it'll get handled as an empty string without quotes.
						i = s.SkipWhiteSpace(i + 1);
						if (i == s.Length)
						{
							return new Opt<List<string>>(null, string.Concat("Unexpected end of Collection. Was expecting a new collection element, but found end of string"));
						}
						break;
					case ArrayEnd:
						// We're all done now!
						++i;
						return new Opt<List<string>>(vals);
					default:
						// Should never be possible to reach here since we say IndexOf ] and ,
						return new Opt<List<string>>(null, string.Concat("Character at index ", i.ToString(), " was not one of '", ArrayElementDelimiter.ToString(), ArrayEnd.ToString(), "', it was ", c.ToString()));
				}
			}
		}
	}
}

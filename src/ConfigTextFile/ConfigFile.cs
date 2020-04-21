using ConfigTextFile.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConfigTextFile
{
	public class ConfigFile
	{
		private static readonly ConfigFile Empty = new ConfigFile();
		private const char ArrayElementDelimiter = ',';
		private const char ArrayStart = '[';
		private const char ArrayEnd = ']';
		private const char ScopeStart = '{';
		private const char ScopeEnd = '}';
		private const char StringStart = '=';
		private const char CommentStart = '#';
		/// <summary>
		/// ArrayElementDelimiter and ArrayEnd
		/// </summary>
		private static readonly char[] ArrayElementDelimiterAndEnd = new char[] { ArrayElementDelimiter, ArrayEnd };
		/// <summary>
		/// StringStart, ArrayStart, and ScopeStart
		/// </summary>
		private static readonly char[] StartSeparators = new char[] { StringStart, ArrayStart, ScopeStart };
		public ConfigFile()
		{
			Tokens = new Dictionary<string, IConfigValue>();
		}
		public ConfigFile(IDictionary<string, IConfigValue> tokens)
		{
			Tokens = tokens;
		}
		/// <summary>
		/// All tokens, keyed by scope:name.
		/// </summary>
		public IDictionary<string, IConfigValue> Tokens { get; }
		/// <summary>
		/// Retrieves a ConfigValue given a key.
		/// If the ConfigValue does not exist, returns null.
		/// </summary>
		/// <param name="key">The key of the config value</param>
		public IConfigValue? this[string key]
		{
			get
			{
				Tokens.TryGetValue(key, out IConfigValue? value);
				return value;
			}
		}
		public static LoadResult TryLoadFile(string path, Encoding encoding, string scopeDelimiter = ":")
		{
			using StreamReader sin = new StreamReader(path, encoding);
			return TryLoadFile(sin, scopeDelimiter);
		}
		public static LoadResult TryLoadFile(StreamReader reader, string scopeDelimiter = ":")
		{
			string content = reader.ReadToEnd();
			Stack<string> parentScopes = new Stack<string>();
			string currentScope = "";
			Dictionary<string, IConfigValue> values = new Dictionary<string, IConfigValue>();
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
						key = content[(keyStart + 1)..i];
						// Then find a separator
						i = content.IndexOfAnySafe(StartSeparators, i);
						break;
					default:
						// Without quotes, the key's name goes until the separator, sans trailing whitespace
						i = content.IndexOfAnySafe(StartSeparators, i);
						key = content[keyStart..i].TrimEnd();
						break;
					case ScopeEnd:
						// Scope end, pop back to the upper scope and advance to the next character
						if (parentScopes.Count > 0)
						{
							currentScope = parentScopes.Pop();
							++i;
						}
						else
						{
							return new LoadResult(false, Empty, string.Concat("Unexpected ", ScopeEnd.ToString(), " found at " + i));
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
						Opt<string> result = ParseSingleString(content, ref i);
						if (result.Val != null)
						{
							values[currentScope + key] = new SingleConfigValue(result.Val);
						}
						else
						{
							return new LoadResult(false, Empty, result.ErrMsg);
						}
						break;
					// An array of strings
					case ArrayStart:
						Opt<CollectionConfigValue> cresult = ParseCollectionConfigValue(content, ref i);
						if (cresult.Val != null)
						{
							values[currentScope + key] = cresult.Val;
						}
						else
						{
							return new LoadResult(false, Empty, cresult.ErrMsg);
						}
						break;
					// Scope change
					case ScopeStart:
						parentScopes.Push(currentScope);
						currentScope = string.Concat(currentScope, key, scopeDelimiter);
						break;
					// This should never happen since we explicitly search for the above 3 chars
					default:
						return new LoadResult(false, Empty, string.Concat("Found unexpected character ", c.ToString(), " when searching for one of '", StringStart.ToString(), ArrayStart.ToString(), ScopeStart.ToString(), "'"));
				}
				i = content.SkipWhiteSpace(i);
			}
			if (parentScopes.Count == 0)
			{
				return new LoadResult(true, new ConfigFile(values), "");
			}
			else
			{
				return new LoadResult(false, Empty, string.Concat("Unterminated scope(s), the current scope is ", currentScope, ". Make sure you are terminating scopes with }"));
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
				return new Opt<string>(s[from..i++]);
			}
			else
			{
				// Otherwise we read until the end of the line and trim off any excess trailing whitespace
				int from = i;
				i = s.IndexOfNewLine(i);
				return new Opt<string>(s[from..i].TrimEnd());
			}
		}
		private static Opt<CollectionConfigValue> ParseCollectionConfigValue(string s, ref int i)
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
						return new Opt<CollectionConfigValue>(null, string.Concat("Unexpected end of Collection. Was expecting the closing quote '", c.ToString(), "' but found end of string"));
					}
					vals.Add(s[from..i]);
				}
				else
				{
					int from = i;
					i = s.IndexOfAny(ArrayElementDelimiterAndEnd, i);
					if (i == -1)
					{
						return new Opt<CollectionConfigValue>(null, string.Concat("Unexpected end of Collection. Was expecting one of '", ArrayElementDelimiter.ToString(), ArrayEnd.ToString(), "' but found end of string"));
					}
					vals.Add(s[from..i].TrimEnd());
				}
				// Now, we need to find the first instance of an end delimiter, which is either a comma or endDelimiter.
				// This could be the current character we're on so passing just i is correct
				i = s.IndexOfAny(ArrayElementDelimiterAndEnd, i);
				if (i == -1)
				{
					return new Opt<CollectionConfigValue>(null, string.Concat("Unexpected end of Collection. Was expecting one of '", ArrayElementDelimiter.ToString(), ArrayEnd.ToString(), "' but found end of string"));
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
							return new Opt<CollectionConfigValue>(null, string.Concat("Unexpected end of Collection. Was expecting a new collection element, but found end of string"));
						}
						break;
					case ArrayEnd:
						// We're all done now!
						++i;
						return new Opt<CollectionConfigValue>(new CollectionConfigValue(vals));
					default:
						// Should never be possible to reach here since we say IndexOf ] and ,
						return new Opt<CollectionConfigValue>(null, string.Concat("Character at index ", i.ToString(), " was not one of '", ArrayElementDelimiter.ToString(), ArrayEnd.ToString(), "', it was ", c.ToString()));
				}
			}
		}
	}
}

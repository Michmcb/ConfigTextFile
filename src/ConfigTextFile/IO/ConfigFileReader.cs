﻿using System;
using System.IO;
using System.Text;

namespace ConfigTextFile.IO
{
	/// <summary>
	/// A forward-only reader for a ConfigFile. Ensures that it is syntactically valid as it reads.
	/// If it encounters something syntactically invalid, it will throw an exception when you attempt to read it.
	/// </summary>
	public sealed class ConfigFileReader : IDisposable
	{
		/// <summary>
		/// Creates a new ConfigFileReader which reads from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The StreamReader to read from</param>
		/// <param name="closeInput">If true, disposes of <paramref name="reader"/> when this object is disposed of</param>
		public ConfigFileReader(StreamReader reader, bool closeInput = true)
		{
			Reader = reader;
			CloseInput = closeInput;
			State = ReadState.Expecting_Key_Comment_EndSection_EndFile;
			SectionLevel = 0;
		}
		/// <summary>
		/// The underlying Reader being used. Fiddling around with this is a great way to cause errors so you shouldn't do that
		/// </summary>
		public StreamReader Reader { get; }
		/// <summary>
		/// If true, Reader will be closed when this is disposed. Otherwise, it will not.
		/// </summary>
		public bool CloseInput { get; set; }
		/// <summary>
		/// Returns true if the State is not EndOfFile (Not if Reader is at the end of stream)
		/// </summary>
		public bool MoreToRead => State != ReadState.EndOfFile;
		/// <summary>
		/// The current state of this reader. Indicates what it can currently read.
		/// </summary>
		public ReadState State { get; private set; }
		/// <summary>
		/// The current nesting level of sections. Initially it is 0.
		/// A ConfigFileFormatException is thrown if the end of the file is reached but there are still sections to close.
		/// </summary>
		public int SectionLevel { get; private set; }
		/// <summary>
		/// Reads a single token, returning its value and the type.
		/// Some tokens don't have values. These are: Start Array, End Array, Start Section, End Section, Finish
		/// </summary>
		public ReadCfgToken Read()
		{
			switch (State)
			{
				case ReadState.Expecting_Key_Comment_EndSection_EndFile:
					{
						char? c = SkipWhiteSpaceAndGetNextChar(Reader);
						// End of the file
						if (!c.HasValue)
						{
							State = ReadState.EndOfFile;
							if (SectionLevel != 0)
							{
								throw new ConfigFileFormatException("Found end of file when there were still " + SectionLevel.ToString() + " sections to close");
							}
							return new ReadCfgToken("", ConfigFileToken.Finish);
						}
						if (c == SyntaxCharacters.CommentStart)
						{
							State = ReadState.Expecting_Key_Comment_EndSection_EndFile;
							return new ReadCfgToken(Reader.ReadLine().TrimEnd(), ConfigFileToken.Comment);
						}
						if (c == SyntaxCharacters.SectionEnd)
						{
							State = ReadState.Expecting_Key_Comment_EndSection_EndFile;
							--SectionLevel;
							if (SectionLevel < 0)
							{
								throw new ConfigFileFormatException("Found } (section close) when there was no section to close.");
							}
							return new ReadCfgToken("", ConfigFileToken.EndSection);
						}
						// In all other cases it's either a quoted or unquoted key
						if (IsQuote(c.Value))
						{
							string str = ReadQuotedString(Reader, c.Value);
							c = SkipWhiteSpaceAndGetNextChar(Reader);
							// If we hit the end of file, that's bad in this case
							if (!c.HasValue)
							{
								throw new ConfigFileFormatException("Encountered end of file after reading the key " + str);
							}
							CheckCharAndUpdateStateAfterKey(c.Value);
							return new ReadCfgToken(str, ConfigFileToken.Key);
						}
						else
						{
							// A Key, read until we find an end of line, or any of: = [ { \r \n
							(string str, char? nextChar) = ReadStringUntil(Reader, c.Value, SyntaxCharacters.EndOfKey);
							// If we hit the end of file, that's bad in this case
							if (!nextChar.HasValue)
							{
								throw new ConfigFileFormatException("Encountered end of file after reading the key " + str);
							}
							// If we encountered an end of line, then we'll keep reading until we find what the next token is
							if (char.IsWhiteSpace(nextChar.Value))
							{
								nextChar = SkipWhiteSpaceAndGetNextChar(Reader);
							}
							CheckCharAndUpdateStateAfterKey(nextChar.Value);
							return new ReadCfgToken(str, ConfigFileToken.Key);
						}
					}
				case ReadState.AtStartOfSection:
					State = ReadState.Expecting_Key_Comment_EndSection_EndFile;
					return new ReadCfgToken("", ConfigFileToken.StartSection);
				case ReadState.AtStartOfArray:
					State = ReadState.ReadingArray;
					return new ReadCfgToken("", ConfigFileToken.StartArray);
				case ReadState.AtEndOfArray:
					State = ReadState.Expecting_Key_Comment_EndSection_EndFile;
					return new ReadCfgToken("", ConfigFileToken.EndArray);
				case ReadState.AtStartOfArrayOrValue:
					{
						// A singular value. Either quoted or unquoted.
						char? c = SkipWhiteSpaceAndGetNextChar(Reader);
						if (!c.HasValue)
						{
							throw new ConfigFileFormatException("Encountered end of file when trying to read Value or Array after a key");
						}
						if (c == SyntaxCharacters.ArrayStart)
						{
							State = ReadState.ReadingArray;
							return new ReadCfgToken("", ConfigFileToken.StartArray);
						}
						if (IsQuote(c.Value))
						{
							// If quoted, the string should not end before we reach the end of the file; it's an error otherwise.
							string str = ReadQuotedString(Reader, c.Value);
							State = ReadState.Expecting_Key_Comment_EndSection_EndFile;
							return new ReadCfgToken(str, ConfigFileToken.Value);
						}
						else
						{
							// If not quoted it's fine to reach the end of the file. However, we don't want to immediately transition to the state EndOfFile,
							// instead we want to wait until next read to ascertain that. The reason why is so that we return ReadState.EndOfFile and transition to State.EndOfFile at the same time.
							(string str, _) = ReadStringUntil(Reader, c.Value, SyntaxCharacters.EndOfLine);
							State = ReadState.Expecting_Key_Comment_EndSection_EndFile;
							return new ReadCfgToken(str, ConfigFileToken.Value);
						}
					}
				case ReadState.ReadingArray:
					{
						// A singular value. Either quoted or unquoted.
						char? c = SkipWhiteSpaceAndGetNextChar(Reader);
						if (!c.HasValue)
						{
							throw new ConfigFileFormatException("Encountered end of file when trying to read Array after a key");
						}
						if (IsQuote(c.Value))
						{
							string str = ReadQuotedString(Reader, c.Value);
							c = SkipWhiteSpaceAndGetNextChar(Reader);
							// If we hit the end of file, that's bad in this case
							if (!c.HasValue)
							{
								throw new ConfigFileFormatException("Encountered end of file after reading the array element " + str);
							}
							CheckCharAndUpdateStateInsideArray(c.Value);
							return new ReadCfgToken(str, ConfigFileToken.ArrayValue);
						}
						else
						{
							(string str, char? nextChar) = ReadStringUntil(Reader, c.Value, SyntaxCharacters.ArrayElementDelimiterAndEnd);
							if (!nextChar.HasValue)
							{
								throw new ConfigFileFormatException("Encountered end of file after reading the array element " + str);
							}
							CheckCharAndUpdateStateInsideArray(nextChar.Value);
							return new ReadCfgToken(str, ConfigFileToken.ArrayValue);
						}
					}
				case ReadState.EndOfFile:
					return new ReadCfgToken("", ConfigFileToken.Finish);
				default:
					throw new InvalidOperationException("State was not a valid value");
			}
		}
		/// <summary>
		/// Returns true if c is ", ', or `
		/// </summary>
		private static bool IsQuote(char c)
		{
			return c == '"' || c == '\'' || c == '`';
		}
		/// <summary>
		/// Skips all whitespace and returns the first non-whitespace character found.
		/// </summary>
		/// <returns>The first non-whitespace character, or null if end of file was found.</returns>
		private static char? SkipWhiteSpaceAndGetNextChar(StreamReader reader)
		{
			while (true)
			{
				int r = reader.Read();
				if (r != -1)
				{
					char c = (char)r;
					if (!char.IsWhiteSpace(c))
					{
						return c;
					}
				}
				else
				{
					return null;
				}
			}
		}
		/// <summary>
		/// Reads a string until <paramref name="quoteChar"/> is encountered.
		/// </summary>
		private static string ReadQuotedString(StreamReader reader, char quoteChar)
		{
			StringBuilder sb = new StringBuilder();
			while (true)
			{
				int r = reader.Read();
				if (r != -1)
				{
					char c = (char)r;
					// Consider the key done once we hit the matching quote character
					if (c != quoteChar)
					{
						sb.Append((char)r);
					}
					else
					{
						// For quoted strings, we include the whitespace at the beginning/end
						return sb.ToString();
					}
				}
				else
				{
					throw new ConfigFileFormatException("Reached end of file before finding the end quote " + quoteChar.ToString());
				}
			}
		}
		/// <summary>
		/// Reads a string until finding one of the characters in the array <paramref name="until"/>.
		/// Prepends <paramref name="firstChar"/> to the returned string, and trims whitespace off the end of the returned string.
		/// </summary>
		private static (string str, char? nextChar) ReadStringUntil(StreamReader reader, char firstChar, char[] until)
		{
			// TODO test perf difference between Span<char> and HashSet<char>
			StringBuilder sb = new StringBuilder();
			sb.Append(firstChar);
			while (true)
			{
				int r = reader.Read();
				if (r != -1)
				{
					char c = (char)r;
					if (!until.ArrayContains(c))
					{
						sb.Append((char)r);
					}
					else
					{
						return (TrimEndStringBuilder(sb), c);
					}
				}
				else
				{
					return (TrimEndStringBuilder(sb), null);
				}
			}
		}
		/// <summary>
		/// Trims whitespace off the end of <paramref name="sb"/> and returns it.
		/// </summary>
		private static string TrimEndStringBuilder(StringBuilder sb)
		{
			int i = sb.Length - 1;
			for (; i > 0 && char.IsWhiteSpace(sb[i]); --i)
			{
			}
			return sb.ToString(0, i + 1);
		}
		private void CheckCharAndUpdateStateInsideArray(char c)
		{
			switch (c)
			{
				case SyntaxCharacters.ArrayEnd:
					State = ReadState.AtEndOfArray;
					break;
				case SyntaxCharacters.ArrayElementDelimiter:
					State = ReadState.ReadingArray;
					break;
				default:
					throw new ConfigFileFormatException("Found unexpected character when searching for next array element or end of array: " + c.ToString());
			}
		}
		private void CheckCharAndUpdateStateAfterKey(char c)
		{
			switch (c)
			{
				case SyntaxCharacters.ValueStart:
					// Can be either one, we don't know until we read the next character (It might be Key=Value or Key=[Values])
					State = ReadState.AtStartOfArrayOrValue;
					break;
				case SyntaxCharacters.ArrayStart:
					State = ReadState.AtStartOfArray;
					break;
				case SyntaxCharacters.SectionStart:
					State = ReadState.AtStartOfSection;
					++SectionLevel;
					break;
			}
		}
		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (CloseInput)
					{
						Reader.Dispose();
					}
				}

				disposedValue = true;
			}
		}
		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}
		#endregion
	}
}

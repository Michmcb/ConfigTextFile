namespace ConfigTextFile.IO
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Writes a formatted <see cref="ConfigFile"/> to a <see cref="StreamWriter"/>.
	/// </summary>
	public sealed class ConfigFileWriter : IDisposable
	{
		private enum StringType
		{
			Key,
			Value,
			ArrayValue
		}
		private string currentIndentation = string.Empty;
		private ConfigFileToken previousWrite = 0;
		/// <summary>
		/// Creates a new <see cref="ConfigFileWriter"/> which writes to <paramref name="writer"/>.
		/// Uses default formatting (<see cref="ConfigFileFormatting.Default"/>).
		/// </summary>
		/// <param name="writer">The stream to write to</param>
		/// <param name="closeOutput">If true, disposes of <paramref name="writer"/> when this object is disposed of</param>
		public ConfigFileWriter(StreamWriter writer, bool closeOutput = true)
		{
			Writer = writer;
			Formatting = ConfigFileFormatting.Default;
			CloseOutput = closeOutput;
			SectionLevel = 0;
			ValidWrites = ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.Finish;
		}
		/// <summary>
		/// Creates a new <see cref="ConfigFileWriter"/> which writes to <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The stram to write to</param>
		/// <param name="formatting">The formatting to use</param>
		/// <param name="closeOutput">If true, disposes of <paramref name="writer"/> when this object is disposed of</param>
		public ConfigFileWriter(StreamWriter writer, ConfigFileFormatting formatting, bool closeOutput = true)
		{
			Writer = writer;
			Formatting = formatting;
			CloseOutput = closeOutput;
			SectionLevel = 0;
			ValidWrites = ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.Finish;
		}
		/// <summary>
		/// The underlying Writer being used. Fiddling around with this is a great way to create malformed files so you shouldn't do that
		/// </summary>
		public StreamWriter Writer { get; }
		/// <summary>
		/// The formatting to apply to the file. Can be changed at any time.
		/// </summary>
		public ConfigFileFormatting Formatting { get; set; }
		/// <summary>
		/// If true, Writer will be closed when this is disposed. Otherwise, it will not.
		/// </summary>
		public bool CloseOutput { get; set; }
		/// <summary>
		/// A set of flags indicating which tokens are currently syntactically valid to write.
		/// If you try and write something that's not valid, a ConfigFileFormatException will be thrown.
		/// </summary>
		public ConfigFileToken ValidWrites { get; private set; }
		/// <summary>
		/// The current nesting level of sections. Initially it is 0.
		/// A ConfigFileFormatException is thrown if Finished is called but there are still sections to close.
		/// </summary>
		public int SectionLevel { get; private set; }
		/// <summary>
		/// Just adds a blank line. Valid to call at any time.
		/// </summary>
		[Obsolete("Prefer WriteLine() instead - it's consistent naming")]
		public void WriteBlankLine()
		{
			Writer.WriteLine();
		}
		/// <summary>
		/// Just adds a blank line. Valid to call at any time.
		/// </summary>
		public void WriteLine()
		{
			Writer.WriteLine();
		}
		/// <summary>
		/// Writes <paramref name="key"/> which can be used to identify a Value, Array or Section.
		/// If <paramref name="key"/> requires it (or if Formatting specifies always quote) then it will be quoted.
		/// </summary>
		/// <param name="key">The key to write</param>
		public void WriteKey(string key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key), nameof(key) + " cannot be null");
			if (key.Length == 0) throw new ArgumentException(nameof(key) + " cannot be an empty string", nameof(key));
			if (CanWrite(ConfigFileToken.Key))
			{
				WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.Key);
				Writer.Write(currentIndentation);
				WriteStr(key, StringType.Key);
				ValidWrites = ConfigFileToken.StartArray | ConfigFileToken.StartSection | ConfigFileToken.Value;
				previousWrite = ConfigFileToken.Key;
			}
			else
			{
				throw CannotWrite(ConfigFileToken.Key);
			}
		}
		/// <summary>
		/// Writes a comment. If <paramref name="text"/> contains any newlines, the comment will be split over multiple lines,
		/// and each line will be a comment.
		/// </summary>
		/// <param name="text">The text of the comment</param>
		public void WriteComment(string text)
		{
			text ??= string.Empty;
			// Because this overload allocates a lot of substrings we call the Span version which doesn't allocate if we're able to do so
#if NETSTANDARD2_0
			if (CanWrite(ConfigFileToken.Comment))
			{
				WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.Comment);
				int indexOfNewline = text.IndexOfAny(SyntaxCharacters.EndOfLine);
				if (indexOfNewline == -1)
				{
					Writer.Write(currentIndentation);
					Writer.Write(SyntaxCharacters.CommentStart);
					Writer.WriteLine(text);
				}
				else
				{
					// The comment spans multiple lines, so break it up and write # before each line
					// By splitting on \n and trimming off any trailing \r, we can cleanly handle both \n and \r\n.
					// That doesn't handle only \r, but that's fine, only old macs use that so we don't have to complicate this method with that.
					string[] lines = text.Split('\n');
					foreach (string line in lines)
					{
						Writer.Write(currentIndentation);
						Writer.Write(SyntaxCharacters.CommentStart);
						Writer.WriteLine(line.TrimEnd('\r'));
					}
				}
				previousWrite = ConfigFileToken.Comment;
			}
			else
			{
				throw CannotWrite(ConfigFileToken.Comment);
			}
#else
			WriteComment(text.AsSpan());
#endif
		}
		/// <summary>
		/// Writes a single Value into an Array or as the value of a Key.
		/// If <paramref name="value"/> requires it (or if Formatting specifies always quote) then it will be quoted.
		/// </summary>
		/// <param name="value">The value to write</param>
		public void WriteValue(string value)
		{
			value ??= string.Empty;
			if (CanWrite(ConfigFileToken.Value))
			{
				Writer.Write(SyntaxCharacters.ValueStart);
				if (WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.Value))
				{
					Writer.Write(currentIndentation);
					Writer.Write(Formatting.Indentation);
				}
				WriteStr(value, StringType.Value);
				Writer.WriteLine();
				ValidWrites = SectionLevel > 0
					? ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.EndSection
					: ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.Finish;
				previousWrite = ConfigFileToken.Value;
				return;
			}
			else if (CanWrite(ConfigFileToken.ArrayValue))
			{
				// Only write the delimiter if the previous token written was also a value within the array
				if (previousWrite == ConfigFileToken.ArrayValue)
				{
					Writer.Write(SyntaxCharacters.ArrayElementDelimiter);
				}
				if (WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.ArrayValue))
				{
					Writer.Write(currentIndentation);
					Writer.Write(Formatting.Indentation);
					WriteStr(value, StringType.ArrayValue);
				}
				else
				{
					// Space between array values if we're not writing blank lines
					if (previousWrite == ConfigFileToken.ArrayValue)
					{
						Writer.Write(' ');
					}
					WriteStr(value, StringType.ArrayValue);
				}
				ValidWrites = ConfigFileToken.ArrayValue | ConfigFileToken.EndArray;
				previousWrite = ConfigFileToken.ArrayValue;
				return;
			}
			else
			{
				throw CannotWrite(ConfigFileToken.Value);
			}
		}
#if !NETSTANDARD2_0
		/// <summary>
		/// Writes <paramref name="key"/> which can be used to identify a Value, Array or Section.
		/// If <paramref name="key"/> requires it (or if Formatting specifies always quote) then it will be quoted.
		/// </summary>
		/// <param name="key">The key to write</param>
		public void WriteKey(in ReadOnlySpan<char> key)
		{
			if (key.Length == 0) throw new ArgumentException(nameof(key) + " cannot be an empty string", nameof(key));
			if (CanWrite(ConfigFileToken.Key))
			{
				WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.Key);
				Writer.Write(currentIndentation);
				WriteStr(key, StringType.Key);
				ValidWrites = ConfigFileToken.StartArray | ConfigFileToken.StartSection | ConfigFileToken.Value;
				previousWrite = ConfigFileToken.Key;
			}
			else
			{
				throw CannotWrite(ConfigFileToken.Key);
			}
		}
		/// <summary>
		/// Writes a comment. If <paramref name="text"/> contains any newlines, the comment will be split over multiple lines,
		/// and each line will be a comment.
		/// </summary>
		/// <param name="text">The text of the comment</param>
		public void WriteComment(in ReadOnlySpan<char> text)
		{
			if (CanWrite(ConfigFileToken.Comment))
			{
				WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.Comment);
				int indexOfNewline = text.IndexOfAny(SyntaxCharacters.EndOfLine);
				if (indexOfNewline == -1)
				{
					Writer.Write(currentIndentation);
					Writer.Write(SyntaxCharacters.CommentStart);
					Writer.WriteLine(text);
				}
				else
				{
					int from;
					int to = -1;
					// The comment spans multiple lines, so break it up and write # before each line
					// By splitting on \n and trimming off any trailing \r, we can cleanly handle both \n and \r\n.
					// That doesn't handle only \r, but that's fine, only old macs use that so we don't have to complicate this method with that.
					while (true)
					{
						from = to + 1;
						// If from is at least the length of the string, that means that the string ended with a \n
						// In that case, write an extra comment character with nothing, but add a newline
						Writer.Write(currentIndentation);
						if (from >= text.Length)
						{
							Writer.WriteLine(SyntaxCharacters.CommentStart);
							break;
						}
						Writer.Write(SyntaxCharacters.CommentStart);
						int index = text[from..].IndexOf('\n');
						if (index != -1)
						{
							Writer.WriteLine(text[from..(to = from + index)].TrimEnd('\r'));
						}
						else
						{
							// Last one
							Writer.WriteLine(text[from..].TrimEnd('\r'));
							break;
						}
					}
				}
				previousWrite = ConfigFileToken.Comment;
			}
			else
			{
				throw CannotWrite(ConfigFileToken.Comment);
			}
		}
		/// <summary>
		/// Writes a single Value into an Array or as the value of a Key.
		/// If <paramref name="value"/> requires it (or if Formatting specifies always quote) then it will be quoted.
		/// </summary>
		/// <param name="value">The value to write</param>
		public void WriteValue(in ReadOnlySpan<char> value)
		{
			if (CanWrite(ConfigFileToken.Value))
			{
				Writer.Write(SyntaxCharacters.ValueStart);
				if (WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.Value))
				{
					Writer.Write(currentIndentation);
					Writer.Write(Formatting.Indentation);
				}
				WriteStr(value, StringType.Value);
				Writer.WriteLine();
				ValidWrites = SectionLevel > 0
					? ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.EndSection
					: ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.Finish;
				previousWrite = ConfigFileToken.Value;
				return;
			}
			else if (CanWrite(ConfigFileToken.ArrayValue))
			{
				// Only write the delimiter if the previous token written was also a value within the array
				if (previousWrite == ConfigFileToken.ArrayValue)
				{
					Writer.Write(SyntaxCharacters.ArrayElementDelimiter);
				}
				if (WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.ArrayValue))
				{
					Writer.Write(currentIndentation);
					Writer.Write(Formatting.Indentation);
					WriteStr(value, StringType.ArrayValue);
				}
				else
				{
					// Space between array values if we're not writing blank lines
					if (previousWrite == ConfigFileToken.ArrayValue)
					{
						Writer.Write(' ');
					}
					WriteStr(value, StringType.ArrayValue);
				}
				ValidWrites = ConfigFileToken.ArrayValue | ConfigFileToken.EndArray;
				previousWrite = ConfigFileToken.ArrayValue;
				return;
			}
			else
			{
				throw CannotWrite(ConfigFileToken.Value);
			}
		}
#endif
		/// <summary>
		/// Opens a new Section
		/// </summary>
		public void WriteStartSection()
		{
			if (CanWrite(ConfigFileToken.StartSection))
			{
				if (WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.StartSection))
				{
					Writer.Write(currentIndentation);
					Writer.Write(SyntaxCharacters.SectionStart);
					Writer.WriteLine();
				}
				else
				{
					Writer.Write(' ');
					Writer.Write(SyntaxCharacters.SectionStart);
					Writer.WriteLine();
				}

				currentIndentation = string.IsNullOrEmpty(Formatting.Indentation) ? string.Empty : string.Concat(Enumerable.Repeat(Formatting.Indentation, ++SectionLevel));
				// Not allowed to finish until all sections are closed
				ValidWrites = ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.EndSection;
				previousWrite = ConfigFileToken.StartSection;
			}
			else
			{
				throw CannotWrite(ConfigFileToken.StartSection);
			}
		}
		/// <summary>
		/// Closes a Section
		/// </summary>
		public void WriteEndSection()
		{
			if (CanWrite(ConfigFileToken.EndSection))
			{
				WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.EndSection);
				currentIndentation = string.IsNullOrEmpty(Formatting.Indentation) ? string.Empty : string.Concat(Enumerable.Repeat(Formatting.Indentation, --SectionLevel));
				Writer.Write(currentIndentation);
				Writer.WriteLine(SyntaxCharacters.SectionEnd);
				// Only allow writing another end section if there's a section open
				ValidWrites = SectionLevel > 0
					? ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.EndSection
					: ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.Finish;
				previousWrite = ConfigFileToken.EndSection;
			}
			else
			{
				throw CannotWrite(ConfigFileToken.EndSection);
			}
		}
		/// <summary>
		/// Opens a new Array
		/// </summary>
		public void WriteStartArray()
		{
			if (CanWrite(ConfigFileToken.StartArray))
			{
				if (Formatting.WriteEqualsBeforeArray)
				{
					Writer.Write(SyntaxCharacters.ValueStart);
				}
				if (WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.StartArray))
				{
					Writer.Write(currentIndentation);
				}
				Writer.Write(SyntaxCharacters.ArrayStart);
				ValidWrites = ConfigFileToken.ArrayValue | ConfigFileToken.EndArray;
				previousWrite = ConfigFileToken.StartArray;
			}
			else
			{
				throw CannotWrite(ConfigFileToken.StartArray);
			}
		}
		/// <summary>
		/// Closes an Array
		/// </summary>
		public void WriteEndArray()
		{
			if (CanWrite(ConfigFileToken.EndArray))
			{
				if (WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.EndArray))
				{
					Writer.Write(currentIndentation);
				}
				Writer.WriteLine(SyntaxCharacters.ArrayEnd);
				ValidWrites = SectionLevel > 0
					? ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.EndSection
					: ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.Finish;
				previousWrite = ConfigFileToken.EndArray;
			}
			else
			{
				throw CannotWrite(ConfigFileToken.EndArray);
			}
		}
		/// <summary>
		/// Calling this signals that you're done writing. You don't have to call this but it's recommended you do so; it's
		/// a sanity check that you've ended the file correctly. If you have not, this will throw a ConfigFileFormatException.
		/// </summary>
		public void Finished()
		{
			if (CanWrite(ConfigFileToken.Finish))
			{
				if (SectionLevel == 0)
				{
					// Disallow any further writes, but nothing wrong with calling Finished many times
					ValidWrites = ConfigFileToken.Finish;
					return;
				}
				else
				{
					throw new ConfigFileFormatException(string.Concat(nameof(ConfigFileWriter) + " cannot currently finish. You need to close ", SectionLevel, " sections"));
				}
			}
			else
			{
				throw new ConfigFileFormatException(string.Concat(nameof(ConfigFileWriter) + " cannot currently finish. It must currently write: ", ValidWrites));
			}
		}
		/// <summary>
		/// Writes the <paramref name="root"/> and all of its children to the stream, recursively.
		/// This does not enclose <paramref name="root"/> in a section (but all child sections are).
		/// If you need <paramref name="root"/> to be enclosed in its own section, call <see cref="WriteStartSection"/> and <see cref="WriteEndSection"/>.
		/// </summary>
		public void WriteSection(ConfigSectionElement root)
		{
			foreach (IConfigElement e in root.Elements.Values)
			{
				Debug.Assert(e.Type != ConfigElementType.Invalid, "Should never get an invalid element when iterating elements");
				foreach (string comment in e.Comments)
				{
					WriteComment(comment);
				}
				WriteKey(e.Key);
				switch (e.Type)
				{
					case ConfigElementType.String:
						WriteValue(e.Value);
						break;
					case ConfigElementType.Array:
						WriteStartArray();
						foreach (string val in e.AsArrayElement().GetValues())
						{
							WriteValue(val);
						}
						WriteEndArray();
						break;
					case ConfigElementType.Section:
						WriteStartSection();
						WriteSection(e.AsSectionElement());
						WriteEndSection();
						break;
				}
			}
		}
		/// <summary>
		/// Returns true if this writer is in a state such that it can write <paramref name="token"/>, which should be a single flag.
		/// </summary>
		/// <param name="token">Checks to see if the writer can write this token in its current state</param>
		/// <returns>true if a <paramref name="token"/> can be written, false if not.</returns>
		public bool CanWrite(ConfigFileToken token)
		{
			return (ValidWrites & token) == token;
		}
#if NETSTANDARD2_0
		private void WriteStr(string str, StringType type)
#else
		private void WriteStr(in ReadOnlySpan<char> str, StringType type)
#endif
		{
			if (str.Length == 0)
			{
				// When it's an empty string we have to write quotes
				char q = Formatting.QuotesInOrderOfPreference[0];
				Writer.Write(q);
				Writer.Write(q);
				return;
			}
			bool needsQuoting;
			char c = str[0];
			switch (type)
			{
				case StringType.Key:
					// Keys can be either Key=Value, Key [Values], Key { Section }
					// Thus that means =, [, and { create ambiguity if they're contained in the Key
					// And also starting with # would make this a comment instead, so quote that
					// And finally starting with } would be interpreted as closing the section

					// Fine: , ]
					// Starts with these needs quoting: } # " ' `
					// Contains these needs quoting: = [ { \r \n
					needsQuoting = Formatting.AlwaysQuoteValues
						|| c == SyntaxCharacters.SectionEnd
						|| c == SyntaxCharacters.CommentStart
						|| c == '"'
						|| c == '\''
						|| c == '`'
						|| char.IsWhiteSpace(c)
						|| char.IsWhiteSpace(str[str.Length - 1])
#if NETSTANDARD2_0
						|| str.IndexOfAny(SyntaxCharacters.EndOfKey) != -1;
#else
						|| str.Contains(SyntaxCharacters.EndOfKey, StringComparison.Ordinal);
#endif
					break;
				case StringType.Value:
					// A Value is like this: Key=Value
					// It's impossible to start a section, so Key={Value is not ambiguous
					// Key==Value isn't ambiguous. The 1st equals sign means "start value" and the 2nd is part of the Value.
					// Starting Value with [ makes it ambiguous because that's Key=[Value, which looks like an array.

					// Fine: { } # = , ]
					// Starts with these needs quoting: [ " ' `
					// Contains these needs quoting: \r \n
					needsQuoting = Formatting.AlwaysQuoteValues
						|| c == SyntaxCharacters.ArrayStart
						|| c == '"'
						|| c == '\''
						|| c == '`'
						|| char.IsWhiteSpace(c)
						|| char.IsWhiteSpace(str[str.Length - 1])
#if NETSTANDARD2_0
							|| str.IndexOfAny(SyntaxCharacters.EndOfLine) != -1;
#else
							|| str.Contains(SyntaxCharacters.EndOfLine, StringComparison.Ordinal);
#endif
					break;
				case StringType.ArrayValue:
					// Arrays are like this (= is optional): Key=[Value 1, Value 2, Value 3]
					// Pretty much everything inside the array is fine, except for array separators and array close

					// Fine: { } # =
					// Starts with these needs quoting: " ' `
					// Contains these needs quoting: , ] \r \n
					needsQuoting = Formatting.AlwaysQuoteValues
						|| c == '"'
						|| c == '\''
						|| c == '`'
						|| char.IsWhiteSpace(c)
						|| char.IsWhiteSpace(str[str.Length - 1])
#if NETSTANDARD2_0
						|| str.IndexOfAny(SyntaxCharacters.ArrayElementDelimiterAndEndAndEndOfLine) != -1;
#else
						|| str.Contains(SyntaxCharacters.ArrayElementDelimiterAndEndAndEndOfLine, StringComparison.Ordinal);
#endif
					break;
				default:
					needsQuoting = false; // This should never actually happen
					break;
			}
			if (needsQuoting)
			{
				char quote = '\0';
				foreach (char q in Formatting.QuotesInOrderOfPreference)
				{
#if NETSTANDARD2_1
					if (str.IndexOf(q) == -1)
#else
					if (!str.Contains(q))
#endif
					{
						quote = q;
						break;
					}
				}
				if (quote == '\0')
				{
#if NETSTANDARD2_1
					throw new ConfigFileFormatException(string.Concat("Couldn't find an appropriate quote to use for the provided string: ", str.ToString()));
#else
					throw new ConfigFileFormatException(string.Concat("Couldn't find an appropriate quote to use for the provided string: ", str));
#endif
				}
				Writer.Write(quote);
				Writer.Write(str);
				Writer.Write(quote);
			}
			else
			{
				Writer.Write(str);
			}
		}
		/// <summary>
		/// If the last thing written was something that's configured to have an blank line after it, then write that line.
		/// Returns true if a blank line was written, false otherwise
		/// </summary>
		private bool WriteBlankLineIfNeeded(ConfigFileToken prev, ConfigFileToken next)
		{
			if (Formatting.ExtraBlankLines.TryGetValue(prev, out ConfigFileToken flags) && (flags & next) == next)
			{
				Writer.WriteLine();
				return true;
			}
			return false;
		}
		internal ConfigFileFormatException CannotWrite(ConfigFileToken token)
		{
			return new ConfigFileFormatException(string.Concat(nameof(ConfigFileWriter), " cannot currently write a ", token.ToString(), ". It can currently write: ", ValidWrites.ToString()));
		}
		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
		/// <summary>
		/// Disposes of <see cref="Writer"/> if <see cref="CloseOutput"/> is true. Otherwise does nothing.
		/// </summary>
		public void Dispose()
		{
			if (!disposedValue)
			{
				if (CloseOutput)
				{
					Writer.Dispose();
				}

				disposedValue = true;
			}
		}
		#endregion
	}
}

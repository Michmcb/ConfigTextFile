namespace ConfigTextFile.IO
{
	using System;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Writes a formatted <see cref="ConfigFile"/> to a <see cref="StreamWriter"/>.
	/// </summary>
	public sealed class ConfigFileWriter : IDisposable
	{
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
		public void WriteBlankLine()
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
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (key.Length == 0) throw new ArgumentException(nameof(key) + " cannot be an emppty string", nameof(key));
			if (CanWrite(ConfigFileToken.Key))
			{
				WriteBlankLineIfNeeded(previousWrite, ConfigFileToken.Key);
				Writer.Write(currentIndentation);
				WriteString(key, true, false);
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
					int from = 0;
					while (indexOfNewline != -1)
					{
						Writer.Write(currentIndentation);
						Writer.Write(SyntaxCharacters.CommentStart);

						Writer.WriteLine(text.Substring(from, indexOfNewline - from));

						// Find the next line
						from = text.IndexOfNotAny(SyntaxCharacters.EndOfLine, indexOfNewline + 1);
						if (from == -1)
						{
							// The comment ended with a newline, so just write a newline and we're all good
							Writer.Write(currentIndentation);
							Writer.WriteLine(SyntaxCharacters.CommentStart);
							return;
						}
						indexOfNewline = text.IndexOfAny(SyntaxCharacters.EndOfLine, from);
					}
					Writer.Write(currentIndentation);
					Writer.Write(SyntaxCharacters.CommentStart);

					Writer.WriteLine(text.Substring(from, text.Length - from));
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
				WriteString(value, false, false);
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
					WriteString(value, false, true);
				}
				else
				{
					// Space between array values if we're not writing blank lines
					if (previousWrite == ConfigFileToken.ArrayValue)
					{
						Writer.Write(' ');
					}
					WriteString(value, false, true);
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
				ValidWrites = ConfigFileToken.Key | ConfigFileToken.Comment;
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
					? ConfigFileToken.Key | ConfigFileToken.Comment | ConfigFileToken.Finish | ConfigFileToken.EndSection
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
					throw new ConfigFileFormatException(nameof(ConfigFileWriter) + " cannot currently finish. You need to close " + SectionLevel.ToString() + " sections");
				}
			}
			else
			{
				throw new ConfigFileFormatException(nameof(ConfigFileWriter) + " cannot currently finish. It must currently write: " + ValidWrites);
			}
		}
		private void WriteString(string str, bool isKey, bool isArrayMember)
		{
			if (str.Length == 0)
			{
				// Even if it's an empty string we have to write quotes if they always want them
				// null/empty keys are not valid
				if (Formatting.AlwaysQuoteValues)
				{
					char q = Formatting.QuotesInOrderOfPreference[0];
					Writer.Write(q);
					Writer.Write(q);
				}
				return;
			}

			// TODO quoting rules are a bit strict, they could be loosened up a bit. For example, for single values, only starting with a syntax character is bad; containing them partway through is perfectly fine.
			bool needsQuoting;
			if (isKey)
			{
				needsQuoting = Formatting.AlwaysQuoteKeys
					// Keys need to be quoted if they start with a quote, or some syntax characters, or whitespace
					|| str[0] == '"' || str[0] == '\'' || str[0] == '`' || str[0] == SyntaxCharacters.CommentStart || char.IsWhiteSpace(str[0])
					// Or if it contains other certain syntax characters; this is an array (= [ { \r \n)
					|| str.IndexOfAny(SyntaxCharacters.EndOfKey) != -1;
			}
			else
			{
				needsQuoting = Formatting.AlwaysQuoteValues
					// We need to quote the string regardless if it starts with quote character or whitespace char...
					|| str[0] == '"' || str[0] == '\'' || str[0] == '`' || str[0] == SyntaxCharacters.CommentStart || char.IsWhiteSpace(str[0])
					// Or if it contains a newline...
					|| str.IndexOfAny(SyntaxCharacters.EndOfLine) != -1
					// Or if it's an array, it contains an array end or array element delimiter
					|| (isArrayMember && (str.IndexOfAny(SyntaxCharacters.ArrayElementDelimiterAndEnd) != -1));
			}

			if (needsQuoting)
			{
				char quote = '\0';
				foreach (char q in Formatting.QuotesInOrderOfPreference)
				{
					if (str.IndexOf(q) == -1)
					{
						quote = q;
						break;
					}
				}
				if (quote == '\0')
				{
					throw new ConfigFileFormatException("Couldn't find an appropriate quote to use (Order of preference is: " + string.Concat(Formatting.QuotesInOrderOfPreference) + ") for the provided string: " + str);
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
		/// Returns true if this writer is in a state such that it can write <paramref name="token"/>, which should be a single flag.
		/// </summary>
		/// <param name="token">Checks to see if the writer can write this token in its current state</param>
		/// <returns>true if a <paramref name="token"/> can be written, false if not.</returns>
		public bool CanWrite(ConfigFileToken token)
		{
			return (ValidWrites & token) == token;
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

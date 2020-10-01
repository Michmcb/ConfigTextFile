namespace ConfigTextFile.IO.Fluent
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// A section of a config file you can write
	/// </summary>
	public sealed class FluentConfigFileSection
	{
		private readonly ConfigFileWriter writer;
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="writer">The <see cref="ConfigFileWriter"/> to write to.</param>
		/// <param name="sectionLevel">The depth of this section.</param>
		public FluentConfigFileSection(ConfigFileWriter writer, int sectionLevel)
		{
			this.writer = writer;
			SectionLevel = sectionLevel;
		}
		/// <summary>
		/// The depth of the sections. 0 for the root section.
		/// </summary>
		public int SectionLevel { get; }
		/// <summary>
		/// Writes a section with the provided <paramref name="key"/>. That section can be filled in using <paramref name="section"/>.
		/// </summary>
		/// <param name="key">The new section's key.</param>
		/// <param name="section">Action to write stuff in the new section.</param>
		public void WriteSection(string key, Action<FluentConfigFileSection> section)
		{
			writer.WriteKey(key);
			writer.WriteStartSection();
			section(new FluentConfigFileSection(writer, SectionLevel + 1));
			writer.WriteEndSection();
		}
		/// <summary>
		/// Writes a comment.
		/// </summary>
		/// <param name="comment">Comment text.</param>
		public void WriteComment(string comment)
		{
			writer.WriteComment(comment);
		}
		/// <summary>
		/// Writes multiple comments.
		/// </summary>
		/// <param name="comments">Comments texts.</param>
		public void WriteComments(IEnumerable<string> comments)
		{
			foreach (string c in comments)
			{
				writer.WriteComment(c);
			}
		}
		/// <summary>
		/// Writes multiple comments.
		/// </summary>
		/// <param name="comments">Comments texts.</param>
		public void WriteComments(params string[] comments)
		{
			foreach (string c in comments)
			{
				writer.WriteComment(c);
			}
		}
		/// <summary>
		/// Writes a string value (<paramref name="key"/>=<paramref name="value"/>).
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void WriteValue(string key, string value)
		{
			writer.WriteKey(key);
			writer.WriteValue(value);
		}
		/// <summary>
		/// Writes an array of strings.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		public void WriteArray(string key, IEnumerable<string> values)
		{
			writer.WriteKey(key);
			writer.WriteStartArray();
			foreach (string v in values)
			{
				writer.WriteValue(v);
			}
			writer.WriteEndArray();
		}
		/// <summary>
		/// Writes an array of strings.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		public void WriteArray(string key, params string[] values)
		{
			writer.WriteKey(key);
			writer.WriteStartArray();
			foreach (string v in values)
			{
				writer.WriteValue(v);
			}
			writer.WriteEndArray();
		}
	}
}

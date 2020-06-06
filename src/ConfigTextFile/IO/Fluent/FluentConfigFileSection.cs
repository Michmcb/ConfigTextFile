using System;
using System.Collections.Generic;

namespace ConfigTextFile.IO.Fluent
{
	public sealed class FluentConfigFileSection
	{
		private readonly ConfigFileWriter writer;
		public FluentConfigFileSection(ConfigFileWriter writer, int sectionLevel)
		{
			this.writer = writer;
			SectionLevel = sectionLevel;
		}
		public int SectionLevel { get; }
		public void WriteSection(string key, Action<FluentConfigFileSection> section)
		{
			writer.WriteKey(key);
			writer.WriteStartSection();
			section(new FluentConfigFileSection(writer, SectionLevel + 1));
			writer.WriteEndSection();
		}
		public void WriteComment(string value)
		{
			writer.WriteComment(value);
		}
		public void WriteComments(IEnumerable<string> values)
		{
			foreach (string value in values)
			{
				writer.WriteComment(value);
			}
		}
		public void WriteComments(params string[] values)
		{
			foreach (string value in values)
			{
				writer.WriteComment(value);
			}
		}
		public void WriteValue(string key, string value)
		{
			writer.WriteKey(key);
			writer.WriteValue(value);
		}
		public void WriteArray(string key, IEnumerable<string> values)
		{
			writer.WriteKey(key);
			writer.WriteStartArray();
			foreach (string value in values)
			{
				writer.WriteValue(value);
			}
			writer.WriteEndArray();
		}
		public void WriteArray(string key, params string[] values)
		{
			writer.WriteKey(key);
			writer.WriteStartArray();
			foreach (string value in values)
			{
				writer.WriteValue(value);
			}
			writer.WriteEndArray();
		}
	}
}

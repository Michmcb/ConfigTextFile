﻿namespace ConfigTextFile.Test.IO
{
	using ConfigTextFile.IO;
	using System.IO;
	using System.Text;
	using Xunit;

	public sealed class FluentConfigFileWriterTests
	{
		[Fact]
		public void WriteStuff()
		{
			ConfigFileFormatting f = new();
			using FluentConfigFileWriter cfg = new(new ConfigFileWriter(new StreamWriter(new FileStream("FluentTest.cfg", FileMode.Create), Encoding.UTF8), f));
			cfg.Write(root =>
			{
				root.WriteValue("Key1", "Value1");
				root.WriteValue("Key2", "Value2");
				root.WriteValue("Key3", "Value3");
				root.WriteComment("My comment");
				root.WriteComments("Comment1", "Comment2", "Comment3");

				root.WriteArray("Array", "Value1", "Value2", "Value3");

				root.WriteSection("Section", section =>
				{
					root.WriteValue("Key1", "Value1");
					root.WriteValue("Key2", "Value2");
					root.WriteValue("Key3", "Value3");
					root.WriteComment("My comment");
					root.WriteComments("Comment1", "Comment2", "Comment3");

					root.WriteArray("Array", "Value1", "Value2", "Value3");
				});
			});
		}
	}
}

﻿using ConfigTextFile.IO;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ConfigTextFile.Test.IO
{
	public sealed class ConfigFileWriterTests
	{
		[Fact]
		public static void WriteTestFile()
		{
			using MemoryStream ms = new MemoryStream();
			ConfigFileFormatting f = new ConfigFileFormatting()
			{
				ExtraBlankLines = new Dictionary<ConfigFileToken, ConfigFileToken>()
				{
					{ ConfigFileToken.Comment , ConfigFileToken.Comment | ConfigFileToken.Key },
					{ ConfigFileToken.Key, ConfigFileToken.Value | ConfigFileToken.StartSection | ConfigFileToken.StartArray },
					{ ConfigFileToken.StartArray, ConfigFileToken.ArrayValue },
					{ ConfigFileToken.ArrayValue, ConfigFileToken.ArrayValue | ConfigFileToken.EndArray },
					{ ConfigFileToken.Value, ConfigFileToken.Key }
				}
			};
			using (ConfigFileWriter cfg = new ConfigFileWriter(new StreamWriter(ms, Encoding.Unicode), f))
			{
				cfg.WriteComment("Test Comment1");
				cfg.WriteComment("Test Comment2");
				cfg.WriteComment("Test Comment3");
				cfg.WriteKey("Key1");
				cfg.WriteValue("Value1");
				cfg.WriteKey("Key2");
				cfg.WriteValue("Value2");
				cfg.WriteKey("Key3");
				cfg.WriteValue("Value3");

				cfg.WriteKey("Section");
				cfg.WriteStartSection();
				{
					cfg.WriteComment(" Test Comment1");
					cfg.WriteComment(" Test Comment2");
					cfg.WriteKey("Key1");
					cfg.WriteValue("Value4");
					cfg.WriteKey("Key2");
					cfg.WriteValue("Value5");

					cfg.WriteKey("Array");
					cfg.WriteStartArray();
					cfg.WriteValue("aValue1");
					cfg.WriteValue("aValue2");
					cfg.WriteValue("aValue3");
					cfg.WriteEndArray();

					cfg.WriteKey("Section");
					cfg.WriteStartSection();
					{
						cfg.WriteKey("Key3");
						cfg.WriteValue("Value6");
					}
					cfg.WriteEndSection();
				}
				cfg.WriteEndSection();
			}
			File.WriteAllBytes("WriteTestFile.cfg", ms.ToArray());
			ConfigFile x = ConfigFile.LoadFile("WriteTestFile.cfg", Encoding.Unicode);
			Assert.Equal("Value1", x["Key1"]);
			Assert.Equal("Value2", x["Key2"]);
			Assert.Equal("Value3", x["Key3"]);

			Assert.Equal("Value4", x["Section:Key1"]);
			Assert.Equal("Value5", x["Section:Key2"]);
			Assert.Equal("aValue1", x["Section:Array:0"]);
			Assert.Equal("aValue2", x["Section:Array:1"]);
			Assert.Equal("aValue3", x["Section:Array:2"]);
			Assert.Equal("Value6", x["Section:Section:Key3"]);

			ConfigStringElement key1 = x.GetElement("Key1").AsStringElement();
			Assert.Equal(3, key1.Comments.Count);
			key1 = x.GetElement("Section:Key1").AsStringElement();
			Assert.Equal(2, key1.Comments.Count);
		}
		[Fact]
		public static void WriteAFileAndMakeSureBadStuffFails()
		{
			using MemoryStream ms = new MemoryStream();
			using (ConfigFileWriter cfg = new ConfigFileWriter(new StreamWriter(ms, Encoding.Unicode)))
			{
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteValue(""));
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartArray());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndArray());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartSection());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndSection());

				cfg.WriteComment("Test Comment");
				cfg.WriteComment(@"Test Comment
That spans many lines
Look at me go!");
				cfg.WriteKey("Key");
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteKey("key"));
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteComment(""));
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndArray());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndSection());
				Assert.Throws<ConfigFileFormatException>(() => cfg.Finished());
				cfg.WriteValue("Value");

				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteValue(""));
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartArray());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndArray());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartSection());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndSection());

				cfg.WriteKey("Key2");
				cfg.WriteValue("Value2");
				cfg.WriteKey("section");
				cfg.WriteStartSection();
				{
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteValue(""));
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartArray());
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndArray());
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartSection());
					Assert.Throws<ConfigFileFormatException>(() => cfg.Finished());

					cfg.WriteKey("'Key3");
					cfg.WriteValue("Value3");

					cfg.WriteKey("#nested");
					cfg.WriteStartSection();
					{
						cfg.WriteComment("Smart Key");
						cfg.WriteComment("Super Smart!");
						cfg.WriteKey("`Key3");
						cfg.WriteValue("Value3");
						cfg.WriteComment(@"This is a stupid key
Very Stupid
So Stupid");
						cfg.WriteKey("#Key4");
						cfg.WriteValue("Value4");
						cfg.WriteKey("Key5");
						cfg.WriteValue("Value5");
					}
					cfg.WriteEndSection();

					cfg.WriteKey("Array");
					cfg.WriteStartArray();

					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteKey("key"));
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteComment(""));
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartArray());
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartSection());
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndSection());
					Assert.Throws<ConfigFileFormatException>(() => cfg.Finished());

					cfg.WriteValue("Value1");
					cfg.WriteValue("Value2");
					cfg.WriteValue("Value3");
					cfg.WriteEndArray();

					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteValue(""));
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartArray());
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndArray());
					Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartSection());
					Assert.Throws<ConfigFileFormatException>(() => cfg.Finished());
				}
				cfg.WriteEndSection();
				cfg.WriteKey(" Key5");
				cfg.WriteValue("Value5");
				cfg.Finished();

				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteKey("key"));
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteComment(""));
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartArray());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndArray());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartSection());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndSection());

				cfg.Finished();
			}
			File.WriteAllBytes("TestAllTheStuff.cfg", ms.ToArray());
		}
	}
}

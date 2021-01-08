namespace ConfigTextFile.Test.IO
{
	using ConfigTextFile.IO;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using Xunit;

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
			var root = x.Root;
			Assert.Equal("Value1", root["Key1"]);
			Assert.Equal("Value2", root["Key2"]);
			Assert.Equal("Value3", root["Key3"]);

			Assert.Equal("Value4",  root.FindElement("Section:Key1").Value);
			Assert.Equal("Value5",  root.FindElement("Section:Key2").Value);
			Assert.Equal("aValue1", root.FindElement("Section:Array:0").Value);
			Assert.Equal("aValue2", root.FindElement("Section:Array:1").Value);
			Assert.Equal("aValue3", root.FindElement("Section:Array:2").Value);
			Assert.Equal("Value6",  root.FindElement("Section:Section:Key3").Value);

			ConfigStringElement key1 = root.GetElement("Key1").AsStringElement();
			Assert.Equal(3, key1.Comments.Count);
			key1 = root.FindElement("Section:Key1").AsStringElement();
			Assert.Equal(2, key1.Comments.Count);
		}
		[Fact]
		public static void WriteAFileAndMakeSureBadStuffFails()
		{
			using MemoryStream ms = new();
			using (ConfigFileWriter cfg = new(new StreamWriter(ms, Encoding.Unicode, leaveOpen: true)))
			{
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteValue(""));
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartArray());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndArray());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteStartSection());
				Assert.Throws<ConfigFileFormatException>(() => cfg.WriteEndSection());

				cfg.WriteComment("Test Comment");
				cfg.WriteComment("Test Comment\nThat spans many lines\nLook at me go!");
				cfg.WriteComment("A Comment\nThat has both types of newline\r\nAnd ends with a newline\n");
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
						cfg.WriteComment("This is a stupid key\nVery Stupid\nSo Stupid");
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

namespace ConfigTextFile.Test
{
	using ConfigTextFile.IO;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using Xunit;

	public sealed class ConfigFileTests
	{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
		[Fact]
		public void WellFormedTest()
		{
			LoadResult loaded = ConfigFile.TryLoadFile("WellFormedConfigTextFile.cfg", Encoding.UTF8);
			ConfigFile? ctf = loaded.ConfigTextFile;
			Assert.NotNull(loaded.ConfigTextFile);

			ConfigSectionElement root = ctf.Root;
			ConfigStringElement val = (ConfigStringElement)root.Elements["First KEy"];
			Assert.Equal("blah blah", val.Value);
			val = (ConfigStringElement)root.Elements["ValueAfterLastScopeEnds"];
			Assert.Equal("Blah blah", val.Value);
			ConfigSectionElement global = (ConfigSectionElement)root.Elements["global"];
			Assert.Equal(9, global.Elements.Count);

			val = (ConfigStringElement)root.GetElement("First KEy");
			Assert.Equal("blah blah", val.Value);
			val = (ConfigStringElement)root.GetElement("ValueAfterLastScopeEnds");
			Assert.Equal("Blah blah", val.Value);

			global = (ConfigSectionElement)root.GetElement("global");
			Assert.Equal(9, global.Elements.Count);
			Assert.Equal("true", global["Value"]);
			Assert.Equal("12345", global["Value Two"]);
			val = (ConfigStringElement)global.Elements["Multiline'd Value"];
			Assert.Equal("Hello,\r\nThis is a long string\nthat spans many lines", val.Value);
			Assert.Equal(2, val.Comments.Count);
			Assert.Contains(" This has got lots of text here", val.Comments);
			Assert.Contains(" Make sure you keep it in quotes!", val.Comments);
			Assert.Contains("myscope", global.Elements);
			Assert.Equal("Hello World!", global["Value=3"]);
			ConfigArrayElement array = (ConfigArrayElement)global.Elements["ArrayValue"];
			Assert.Equal(5, array.Elements.Count);
			{
				Assert.Equal("One", array["0"]);
				Assert.Equal("Three and Four]]]", array["1"]);
				Assert.Equal("Three and Four", array["2"]);
				Assert.Equal("end []", array["3"]);
				Assert.Equal("Last value", array["4"]);
			}
			Assert.Contains("AnArray", global.Elements);
			Assert.Equal(1, global.Elements["AnArray"].AsArrayElement().Elements.Count);
			Assert.Equal("[String]", global["NotArray"]);

			IConfigElement emptyArray = Assert.Contains("EmptyArray", global.Elements);
			Assert.Empty(emptyArray.AsArrayElement().Elements);

			ConfigSectionElement myscope = (ConfigSectionElement)root.FindElement("global:myscope");
			Assert.Equal(2, myscope.Elements.Count);
			Assert.Equal("12345", myscope["Quoted Value"]);
			Assert.Contains("nested scope", myscope.Elements);

			ConfigSectionElement nested_scope = (ConfigSectionElement)root.FindElement("global:myscope:nested scope");
			Assert.Equal(1, nested_scope.Elements.Count);
			Assert.Equal("=Yes", nested_scope["Value with Equals Sign"]);

			Assert.Same(global, root.GetElement("global"));
			Assert.Same(myscope, root.FindElement("global:myscope"));
			Assert.Same(myscope, global.Elements["myscope"]);
			Assert.Same(myscope, global.GetElement("myscope"));
			Assert.Same(nested_scope, root.FindElement("global:myscope:nested scope"));
			Assert.Same(nested_scope, myscope.Elements["nested scope"]);
			Assert.Same(nested_scope, myscope.GetElement("nested scope"));
			Assert.Same(array, root.FindElement("global:ArrayValue"));
			Assert.Same(array, root.FindElement("global:ArrayValue"));

			Assert.Throws<KeyNotFoundException>(() => root.FindElement("global:"));
			Assert.Same(ConfigInvalidElement.Inst, root.TryFindElement("global:"));
		}
		[Fact]
		public void MalformedFiles()
		{
			LoadResult lr = ConfigFile.TryLoadFile("KeyWithoutValue.cfg", Encoding.UTF8);
			Assert.Null(lr.ConfigTextFile);
			lr = ConfigFile.TryLoadFile("LoneKey.cfg", Encoding.UTF8);
			Assert.Null(lr.ConfigTextFile);
			lr = ConfigFile.TryLoadFile("TooManyScopeTerminators.cfg", Encoding.UTF8);
			Assert.Null(lr.ConfigTextFile);
			lr = ConfigFile.TryLoadFile("UnterminatedArray.cfg", Encoding.UTF8);
			Assert.Null(lr.ConfigTextFile);
			lr = ConfigFile.TryLoadFile("UnterminatedScope.cfg", Encoding.UTF8);
			Assert.Null(lr.ConfigTextFile);
		}
		[Fact]
		public void Reader()
		{
			using ConfigFileReader reader = new ConfigFileReader(new System.IO.StreamReader("WellFormedConfigTextFile.cfg", Encoding.UTF8));
			ReadAndAssertToken(reader, "First KEy", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "blah blah", ConfigFileToken.Value);
			ReadAndAssertToken(reader, "global", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "", ConfigFileToken.StartSection);
			ReadAndAssertToken(reader, "Value", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "true", ConfigFileToken.Value);
			ReadAndAssertToken(reader, "Value Two", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "12345", ConfigFileToken.Value);
			ReadAndAssertToken(reader, " This has got lots of text here", ConfigFileToken.Comment);
			ReadAndAssertToken(reader, " Make sure you keep it in quotes!", ConfigFileToken.Comment);
			ReadAndAssertToken(reader, "Multiline'd Value", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "Hello,\r\nThis is a long string\nthat spans many lines", ConfigFileToken.Value);
			ReadAndAssertToken(reader, "myscope", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "", ConfigFileToken.StartSection);
			ReadAndAssertToken(reader, "Quoted Value", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "12345", ConfigFileToken.Value);
			ReadAndAssertToken(reader, "nested scope", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "", ConfigFileToken.StartSection);
			ReadAndAssertToken(reader, "Value with Equals Sign", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "=Yes", ConfigFileToken.Value);
			ReadAndAssertToken(reader, "", ConfigFileToken.EndSection);
			ReadAndAssertToken(reader, "", ConfigFileToken.EndSection);
			ReadAndAssertToken(reader, "Value=3", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "Hello World!", ConfigFileToken.Value);
			ReadAndAssertToken(reader, "Comment", ConfigFileToken.Comment);
			ReadAndAssertToken(reader, "ArrayValue", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "", ConfigFileToken.StartArray);
			ReadAndAssertToken(reader, "One", ConfigFileToken.ArrayValue);
			ReadAndAssertToken(reader, "Three and Four]]]", ConfigFileToken.ArrayValue);
			ReadAndAssertToken(reader, "Three and Four", ConfigFileToken.ArrayValue);
			ReadAndAssertToken(reader, "end []", ConfigFileToken.ArrayValue);
			ReadAndAssertToken(reader, "Last value", ConfigFileToken.ArrayValue);
			ReadAndAssertToken(reader, "", ConfigFileToken.EndArray);
			ReadAndAssertToken(reader, "AnArray", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "", ConfigFileToken.StartArray);
			ReadAndAssertToken(reader, "String", ConfigFileToken.ArrayValue);
			ReadAndAssertToken(reader, "", ConfigFileToken.EndArray);
			ReadAndAssertToken(reader, "NotArray", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "[String]", ConfigFileToken.Value);
			ReadAndAssertToken(reader, "EmptyArray", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "", ConfigFileToken.StartArray);
			ReadAndAssertToken(reader, "", ConfigFileToken.EndArray);
			ReadAndAssertToken(reader, "", ConfigFileToken.EndSection);
			ReadAndAssertToken(reader, "ValueAfterLastScopeEnds", ConfigFileToken.Key);
			ReadAndAssertToken(reader, "Blah blah", ConfigFileToken.Value);
			ReadAndAssertToken(reader, "", ConfigFileToken.Finish);
			ReadAndAssertToken(reader, "", ConfigFileToken.Finish);

			Assert.False(reader.MoreToRead);
			Assert.Equal(ReadState.EndOfFile, reader.State);

			static void ReadAndAssertToken(ConfigFileReader reader, string expectedValue, ConfigFileToken expectedToken)
			{
				ReadCfgToken token = reader.Read();
				Assert.Equal(expectedValue, token.Value);
				Assert.Equal(expectedToken, token.Type);
			}
		}
		[Fact]
		public void Save()
		{
			ConfigFile file = new();
			ConfigSectionElement root = file.Root;
			root.AddElement(new ConfigStringElement("Key1", "value1"));
			root.AddElement(new ConfigStringElement("Key2", "value  2"));
			root.AddElement(new ConfigStringElement("Key3", "value~3"));
			root.AddElement(new ConfigArrayElement("ArrayKey", "Value 1", "Valu,e 2", "Va]lue 3!"));
			ConfigSectionElement subSection = new ConfigSectionElement("Subsection");
			root.AddElement(subSection);

			subSection.AddElement(new ConfigStringElement("Key1", "'value1", new string[] { "My comments", "hehey!" }, false));
			subSection.AddElement(new ConfigStringElement("Key2", "\"value  2"));
			subSection.AddElement(new ConfigStringElement("Key3", "`value3"));

			using (ConfigFileWriter w = new ConfigFileWriter(new StreamWriter(new FileStream("test.cfg", FileMode.Create, FileAccess.Write), Encoding.UTF8, leaveOpen: false), closeOutput: true))
			{
				file.Save(w);
			}

			file = ConfigFile.LoadFile("test.cfg", Encoding.UTF8, LoadCommentsPreference.Load);
			root = file.Root;
			ConfigStringElement str = Assert.IsType<ConfigStringElement>(root.Elements["Key1"]);
			Assert.Equal("value1", str.Value);
			str = Assert.IsType<ConfigStringElement>(root.Elements["Key2"]);
			Assert.Equal("value  2", str.Value);
			str = Assert.IsType<ConfigStringElement>(root.Elements["Key3"]);
			Assert.Equal("value~3", str.Value);

			ConfigArrayElement arr = Assert.IsType<ConfigArrayElement>(root.Elements["ArrayKey"]);
			Assert.Collection(arr.GetValues(), x => Assert.Equal("Value 1", x), x => Assert.Equal("Valu,e 2", x), x => Assert.Equal("Va]lue 3!", x));

			subSection = Assert.IsType<ConfigSectionElement>(root.Elements["Subsection"]);
			Assert.Equal(3, subSection.Elements.Count);

			Assert.Collection(subSection.Elements.Values,
				x =>
				{
					str = Assert.IsType<ConfigStringElement>(x);
					Assert.Equal("Key1", str.Key);
					Assert.Equal("'value1", str.Value);
					Assert.Collection(x.Comments, y => Assert.Equal("My comments", y), y => Assert.Equal("hehey!", y));
				},
				x =>
				{
					str = Assert.IsType<ConfigStringElement>(x);
					Assert.Equal("Key2", str.Key);
					Assert.Equal("\"value  2", str.Value);
				},
				x =>
				{
					str = Assert.IsType<ConfigStringElement>(x);
					Assert.Equal("Key3", str.Key);
					Assert.Equal("`value3", str.Value);
				});
		}
		[Fact]
		public void AddingBadStuffSaysNo()
		{
			ConfigSectionElement section = new();
			ConfigStringElement str = new ConfigStringElement("Key", "value");
			section.AddElement(str);
			Assert.Throws<ArgumentException>(() => section.AddElement(new ConfigStringElement("Key", "value")));
			Assert.Throws<ArgumentException>(() => section.AddElement(str));
			Assert.Throws<ArgumentException>(() => section.AddElement(new ConfigArrayElement("Key", "value")));
			Assert.Throws<ArgumentException>(() => section.AddElement(new ConfigSectionElement("Key")));

			ConfigArrayElement array = new ConfigArrayElement("Key1");
			section.AddElement(array);
			Assert.Throws<ArgumentException>(() => section.AddElement(array));

			ConfigSectionElement sect = new ConfigSectionElement("Key2");
			section.AddElement(sect);
			Assert.Throws<ArgumentException>(() => section.AddElement(sect));
		}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
	}
}

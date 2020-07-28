using ConfigTextFile.IO;
using System.Text;
using Xunit;

namespace ConfigTextFile.Test
{
	public class ConfigFileTests
	{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
		[Fact]
		public void WellFormedTest()
		{
			LoadResult loaded = ConfigFile.TryLoadFile("WellFormedConfigTextFile.cfg", Encoding.UTF8);
			ConfigFile ctf = loaded.ConfigTextFile;
			Assert.True(loaded.Success);

			ConfigSectionElement root = ctf.Root;
			ConfigStringElement val = (ConfigStringElement)root.Elements["First KEy"];
			Assert.Equal("blah blah", val.Value);
			val = (ConfigStringElement)root.Elements["ValueAfterLastScopeEnds"];
			Assert.Equal("Blah blah", val.Value);
			ConfigSectionElement global = (ConfigSectionElement)root.Elements["global"];
			Assert.Equal(8, global.Elements.Count);

			val = (ConfigStringElement)ctf.AllElements["First KEy"];
			Assert.Equal("blah blah", val.Value);
			val = (ConfigStringElement)ctf.AllElements["ValueAfterLastScopeEnds"];
			Assert.Equal("Blah blah", val.Value);

			global = (ConfigSectionElement)ctf.AllElements["global"];
			Assert.Equal(8, global.Elements.Count);
			Assert.Equal("true", global["Value"]);
			Assert.Equal("12345", global["Value Two"]);
			val = (ConfigStringElement)global.Elements["Multiline'd Value"];
			Assert.Equal(@"Hello,
This is a long string
that spans many lines", val.Value);
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
			Assert.Equal(1, global.Elements["AnArray"].Elements.Count);
			Assert.Equal("[String]", global["NotArray"]);

			ConfigSectionElement myscope = (ConfigSectionElement)ctf.AllElements["global:myscope"];
			Assert.Equal(2, myscope.Elements.Count);
			Assert.Equal("12345", myscope["Quoted Value"]);
			Assert.Contains("nested scope", myscope.Elements);

			ConfigSectionElement nested_scope = (ConfigSectionElement)ctf.AllElements["global:myscope:nested scope"];
			Assert.Equal(1, nested_scope.Elements.Count);
			Assert.Equal("=Yes", nested_scope["Value with Equals Sign"]);

			Assert.Same(global, ctf.GetElement("global"));
			Assert.Same(myscope, ctf.GetElement("global:myscope"));
			Assert.Same(myscope, global.Elements["myscope"]);
			Assert.Same(myscope, global.GetElement("myscope"));
			Assert.Same(nested_scope, ctf.GetElement("global:myscope:nested scope"));
			Assert.Same(nested_scope, myscope.Elements["nested scope"]);
			Assert.Same(nested_scope, myscope.GetElement("nested scope"));
			Assert.Same(array, ctf.AllElements["global:ArrayValue"]);
			Assert.Same(array, ctf.GetElement("global:ArrayValue"));
		}
		[Fact]
		public void MalformedFiles()
		{
			LoadResult lr = ConfigFile.TryLoadFile("KeyWithoutValue.cfg", Encoding.UTF8);
			Assert.False(lr.Success);
			lr = ConfigFile.TryLoadFile("LoneKey.cfg", Encoding.UTF8);
			Assert.False(lr.Success);
			lr = ConfigFile.TryLoadFile("TooManyScopeTerminators.cfg", Encoding.UTF8);
			Assert.False(lr.Success);
			lr = ConfigFile.TryLoadFile("UnterminatedArray.cfg", Encoding.UTF8);
			Assert.False(lr.Success);
			lr = ConfigFile.TryLoadFile("UnterminatedScope.cfg", Encoding.UTF8);
			Assert.False(lr.Success);
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
			ReadAndAssertToken(reader, "Hello,\r\nThis is a long string\r\nthat spans many lines", ConfigFileToken.Value);
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
#pragma warning restore CS8602 // Dereference of a possibly null reference.
	}
}

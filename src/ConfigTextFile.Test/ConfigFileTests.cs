using Xunit;
using System.Text;

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

			ConfigStringElement val = (ConfigStringElement)ctf.Elements["First KEy"];
			Assert.Equal("blah blah", val.Value);
			val = (ConfigStringElement)ctf.Elements["ValueAfterLastScopeEnds"];
			Assert.Equal("Blah blah", val.Value);

			ConfigSectionElement global = (ConfigSectionElement)ctf.Elements["global"];
			Assert.Equal(8, global.Elements.Count);
			Assert.Equal("true", global["Value"]);
			Assert.Equal("12345", global["Value Two"]);
			Assert.Equal(@"Hello,
This is a long string
that spans many lines", global["Multiline'd Value"]);
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

			ConfigSectionElement myscope = (ConfigSectionElement)ctf.Elements["global:myscope"];
			Assert.Equal(2, myscope.Elements.Count);
			Assert.Equal("12345", myscope["Quoted Value"]);
			Assert.Contains("nested scope", myscope.Elements);

			ConfigSectionElement nested_scope = (ConfigSectionElement)ctf.Elements["global:myscope:nested scope"];
			Assert.Equal(1, nested_scope.Elements.Count);
			Assert.Equal("=Yes", nested_scope["Value with Equals Sign"]);

			Assert.Same(global, ctf.GetSection("global"));
			Assert.Same(myscope, ctf.GetSection("global:myscope"));
			Assert.Same(myscope, global.Elements["myscope"]);
			Assert.Same(myscope, global.GetSection("myscope"));
			Assert.Same(nested_scope, ctf.GetSection("global:myscope:nested scope"));
			Assert.Same(nested_scope, myscope.Elements["nested scope"]);
			Assert.Same(nested_scope, myscope.GetSection("nested scope"));
			Assert.Same(array, ctf.Elements["global:ArrayValue"]);
			Assert.Same(array, ctf.GetSection("global:ArrayValue"));
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
#pragma warning restore CS8602 // Dereference of a possibly null reference.
	}
}

namespace ConfigTextFile.Test.ConfigurationSource
{
	using Microsoft.Extensions.Configuration;
	using ConfigTextFile.ConfigurationSource;
	using Xunit;
	using System.IO;

	public sealed class Test
	{
		[Fact]
		public void LoadWellFormedFullPath()
		{
			IConfigurationRoot root = new ConfigurationBuilder()
				.AddConfigTextFile(Path.GetFullPath("WellFormedConfigTextFile.cfg"))
				.Build();
			AssertLoaded(root);
		}
		[Fact]
		public void LoadWellFormedRelativePath()
		{
			IConfigurationRoot root = new ConfigurationBuilder()
				.AddConfigTextFile("WellFormedConfigTextFile.cfg")
				.Build();
			AssertLoaded(root);
		}
		[Fact]
		public void LoadWellFormedSpecifiedDirectory()
		{
			IConfigurationRoot root = new ConfigurationBuilder()
				.SetBasePath(Path.GetFullPath("."))
				.AddConfigTextFile("WellFormedConfigTextFile.cfg")
				.Build();
			AssertLoaded(root);
		}
		private static void AssertLoaded(IConfigurationRoot root)
		{
			Assert.Equal("blah blah", root["First KEy"]);
			Assert.Equal("true", root["global:Value"]);
			Assert.Equal("12345", root["global:Value Two"]);
			Assert.Equal("Hello,\r\nThis is a long string\nthat spans many lines", root["global:Multiline'd Value"]);
			Assert.Equal("12345", root["global:myscope:Quoted Value"]);
			Assert.Equal("=Yes", root["global:myscope:nested scope:Value with Equals Sign"]);
			Assert.Equal("Hello World!", root["global:Value=3"]);

			Assert.Equal("One", root["global:ArrayValue:0"]);
			Assert.Equal("Three and Four]]]", root["global:ArrayValue:1"]);
			Assert.Equal("Three and Four", root["global:ArrayValue:2"]);
			Assert.Equal("end []", root["global:ArrayValue:3"]);
			Assert.Equal("Last value", root["global:ArrayValue:4"]);

			Assert.Equal("String", root["global:AnArray:0"]);
			Assert.Equal("[String]", root["global:NotArray"]);
			Assert.Empty(root.GetSection("global:EmptyArray").GetChildren());
			Assert.Null(root["global:EmptyArray:0"]);
			Assert.Equal("Blah blah", root["ValueAfterLastScopeEnds"]);
		}
	}
}

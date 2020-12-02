namespace ConfigTextFile.Test
{
	using System;
	using Xunit;

	public sealed class ConfigPathTests
	{
		[Fact]
		public void Joining()
		{
			Assert.Equal("Path:Key", ConfigPath.Join("Path", "Key"));
			Assert.Equal("Path:Key", ConfigPath.Join("Path:", "Key"));
			Assert.Equal("Path:Path2:Key", ConfigPath.Join("Path:Path2", "Key"));
			Assert.Equal("Path:Path2:Key", ConfigPath.Join("Path:Path2:", "Key"));
			Assert.Equal("Key", ConfigPath.Join("", "Key"));
			Assert.Equal("Key", ConfigPath.Join(null, "Key"));
			Assert.Throws<ArgumentException>(() => ConfigPath.Join("Path", "Ke:y"));
		}
	}
}

using System;
using System.Collections.Generic;
using Xunit;

namespace ConfigTextFile.Test
{
	public class ConfigFileTests
	{
#pragma warning disable CS8602 // Dereference of a possibly null reference.
		[Fact]
		public void WellFormedTest()
		{
			LoadResult loaded = ConfigFile.TryLoadFile("WellFormedConfigTextFile.cfg", System.Text.Encoding.UTF8);
			ConfigFile ctf = loaded.ConfigTextFile;
			Assert.True(loaded.Success);

			IConfigValue? val = ctf["global:Value"];
			Assert.NotNull(val);
			Assert.True(val.IsSingular);
			Assert.False(val.IsCollection);
			Assert.Equal("true", val.Singular);
			Assert.Throws<InvalidOperationException>(() => val.Collection);

			val = ctf["global:Value Two"];
			Assert.NotNull(val);
			Assert.True(val.IsSingular);
			Assert.False(val.IsCollection);
			Assert.Equal("12345", val.Singular);
			Assert.Throws<InvalidOperationException>(() => val.Collection);

			val = ctf["global:Multiline'd Value"];
			Assert.NotNull(val);
			Assert.True(val.IsSingular);
			Assert.False(val.IsCollection);
			Assert.Equal(@"Hello,
This is a long string
that spans many lines", val.Singular);
			Assert.Throws<InvalidOperationException>(() => val.Collection);

			val = ctf["global:myscope:Quoted Value"];
			Assert.NotNull(val);
			Assert.True(val.IsSingular);
			Assert.False(val.IsCollection);
			Assert.Equal("12345", val.Singular);
			Assert.Throws<InvalidOperationException>(() => val.Collection);

			val = ctf["global:myscope:nested scope:Value with Equals Sign"];
			Assert.NotNull(val);
			Assert.True(val.IsSingular);
			Assert.False(val.IsCollection);
			Assert.Equal("=Yes", val.Singular);
			Assert.Throws<InvalidOperationException>(() => val.Collection);

			val = ctf["global:Value=3"];
			Assert.NotNull(val);
			Assert.True(val.IsSingular);
			Assert.False(val.IsCollection);
			Assert.Equal("Hello World!", val.Singular);
			Assert.Throws<InvalidOperationException>(() => val.Collection);

			val = ctf["global:ArrayValue"];
			Assert.NotNull(val);
			Assert.False(val.IsSingular);
			Assert.True(val.IsCollection);
			Assert.Throws<InvalidOperationException>(() => val.Singular);
			ICollection<string> collection = val.Collection;
			Assert.Equal(5, collection.Count);
			using IEnumerator<string> iter = collection.GetEnumerator();
			iter.MoveNext();
			Assert.Equal("One", iter.Current);
			iter.MoveNext();
			Assert.Equal("Three and Four]]]", iter.Current);
			iter.MoveNext();
			Assert.Equal("Three and Four", iter.Current);
			iter.MoveNext();
			Assert.Equal("end []", iter.Current);
			iter.MoveNext();
			Assert.Equal("Last value", iter.Current);
		}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
	}
}

namespace ConfigTextFile.Test
{
	using ConfigTextFile;
	using Xunit;

	public sealed class ConfigBuilderTests
	{
		[Fact]
		public void SaveFluent()
		{
			ConfigSectionElement root = new();
			ConfigBuilder.Build(root, a =>
			{
				a.String("Key1", "value1");
				a.String("Key2", "value  2");
				a.String("Key3", "value~3");
				a.Array("ArrayKey", "Value 1", "Valu,e 2", "Va]lue 3!");

				a.Section("Subsection", b =>
				{
					b.String("Key1", "'value1", new string[] { "My comments", "hehey!" }, false);
					b.String("Key2", "\"value  2");
					b.String("Key3", "`value3");
				});
			});

			ConfigStringElement str = Assert.IsType<ConfigStringElement>(root.Elements["Key1"]);
			Assert.Equal("value1", str.Value);
			str = Assert.IsType<ConfigStringElement>(root.Elements["Key2"]);
			Assert.Equal("value  2", str.Value);
			str = Assert.IsType<ConfigStringElement>(root.Elements["Key3"]);
			Assert.Equal("value~3", str.Value);

			ConfigArrayElement arr = Assert.IsType<ConfigArrayElement>(root.Elements["ArrayKey"]);
			Assert.Collection(arr.GetValues(), x => Assert.Equal("Value 1", x), x => Assert.Equal("Valu,e 2", x), x => Assert.Equal("Va]lue 3!", x));

			ConfigSectionElement subSection = Assert.IsType<ConfigSectionElement>(root.Elements["Subsection"]);
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
	}
}

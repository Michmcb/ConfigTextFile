using Xunit;

namespace ConfigTextFile.Test.ConfigInvalidElementTests
{
	public sealed class Other
	{
		[Fact]
		public void Behaviour()
		{
			ConfigInvalidElement e = ConfigInvalidElement.Inst;
			// All these should throw
			Assert.Throws<ConfigInvalidElementException>(() => e[""]);
			Assert.Throws<ConfigInvalidElementException>(() => e[""] = "");
			Assert.Throws<ConfigInvalidElementException>(() => e.Elements);
			Assert.Throws<ConfigInvalidElementException>(() => e.Key);
			Assert.Throws<ConfigInvalidElementException>(() => e.Path);
			Assert.Throws<ConfigInvalidElementException>(() => e.Value = "");
			Assert.Throws<ConfigInvalidElementException>(e.ThrowIfInvalid);
			Assert.Throws<ConfigInvalidElementException>(e.AsArrayElement);
			Assert.Throws<ConfigInvalidElementException>(e.AsSectionElement);
			Assert.Throws<ConfigInvalidElementException>(e.AsStringElement);

			Assert.Equal(ConfigElementType.Invalid, e.Type);
			Assert.False(e.IsValid);

			Assert.Equal(e, e.GetElement(""));
			Assert.Equal(e, e.GetSection(""));
			Assert.Empty(e.GetChildren());

			Assert.Empty(e.Value);
			ConfigInvalidElement.InvalidValue = null;
			Assert.Null(e.Value);
			ConfigInvalidElement.InvalidValue = "Some value";
			Assert.Equal("Some value", e.Value);
		}
	}
}

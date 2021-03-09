namespace ConfigTextFile
{
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Represents that a requested element did not exist.
	/// This is a singleton, and you can't do too much with this.
	/// Check individual methods and properties to see what they will always do.
	/// The singleton instance is <see cref="Inst"/>.
	/// </summary>
	public sealed class ConfigInvalidElement : IConfigElement
	{
		/// <summary>
		/// The singleton instance of <see cref="ConfigInvalidElement"/>.
		/// </summary>
		public static readonly ConfigInvalidElement Inst = new();
		private ConfigInvalidElement() { }
		/// <summary>
		/// Always throws a <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		public string this[string key]
		{
			get => throw new ConfigInvalidElementException();
			set => throw new ConfigInvalidElementException();
		}
		/// <summary>
		/// Returns <see cref="ConfigElementType.Invalid"/>.
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Invalid;
		/// <summary>
		/// Always throws a <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		public string Key => throw new ConfigInvalidElementException();
		/// <summary>
		/// Always throws a <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		public string Path => throw new ConfigInvalidElementException();
		/// <summary>
		/// Always throws <see cref="ConfigInvalidElementException"/> on setting. Always returns <see cref="string.Empty"/>.
		/// </summary>
		public string Value { get => string.Empty; set => throw new ConfigInvalidElementException(); }
		/// <summary>
		/// Returns <paramref name="alternative"/> because this is a <see cref="ConfigInvalidElement"/>.
		/// </summary>
		/// <param name="alternative">Returns this.</param>
		/// <returns><paramref name="alternative"/></returns>
		[return: NotNullIfNotNull("alternative")]
		public string? ValueOr(string? alternative)
		{
			return alternative;
		}
		/// <summary>
		/// Always false.
		/// </summary>
		public bool IsValid => false;
		/// <summary>
		/// Always throws <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		public string? Comments { get => throw new ConfigInvalidElementException(); set => throw new ConfigInvalidElementException(); }
		/// <summary>
		/// Always throws a <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		public IConfigElement GetElement(string key)
		{
			throw new ConfigInvalidElementException();
		}
		/// <summary>
		/// Always returns <see cref="Inst"/>.
		/// </summary>
		public IConfigElement TryGetElement(string key)
		{
			return Inst;
		}
		/// <summary>
		/// Always throws <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		public IConfigElement FindElement(string path)
		{
			throw new ConfigInvalidElementException();
		}
		/// <summary>
		/// Always returns <see cref="Inst"/>.
		/// </summary>
		public IConfigElement TryFindElement(string path)
		{
			return Inst;
		}
		/// <summary>
		/// Always throws a <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		public void ThrowIfInvalid()
		{
			throw new ConfigInvalidElementException();
		}
		/// <summary>
		/// Always throws a <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		public ConfigArrayElement AsArrayElement()
		{
			throw new ConfigInvalidElementException();
		}
		/// <summary>
		/// Always throws a <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		public ConfigSectionElement AsSectionElement()
		{
			throw new ConfigInvalidElementException();
		}
		/// <summary>
		/// Always throws a <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		public ConfigStringElement AsStringElement()
		{
			throw new ConfigInvalidElementException();
		}
	}
}

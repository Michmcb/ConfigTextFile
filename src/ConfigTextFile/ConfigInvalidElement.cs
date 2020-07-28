namespace ConfigTextFile
{
	using System.Collections.Generic;

	/// <summary>
	/// Represents that a requested element did not exist.
	/// This is a singleton, and you can't do too much with this.
	/// Check individual methods and properties to see what they will always do; either empty strings, empty arrays, or throw a ConfigInvalidElementException.
	/// The singleton instance is ConfigInvalidElement.Inst, if you need it.
	/// </summary>
	public sealed class ConfigInvalidElement : IConfigElement
	{
		/// <summary>
		/// This is the value that is returned whenever you try to get the Value of a ConfigInvalidElement.
		/// You can set this to whatever you want.
		/// </summary>
		public static string InvalidValue { get; set; } = string.Empty;
		/// <summary>
		/// The singleton instance of ConfigInvalidElement.
		/// </summary>
		public static readonly ConfigInvalidElement Inst = new ConfigInvalidElement();
		private ConfigInvalidElement() { }
		/// <summary>
		/// Always throws an ConfigInvalidElementException.
		/// </summary>
		public string this[string key]
		{
			get
			{
				throw new ConfigInvalidElementException();
			}
			set
			{
				throw new ConfigInvalidElementException();
			}
		}
		/// <summary>
		/// Returns ConfigElementType.Invalid.
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Invalid;
		/// <summary>
		/// Always throws a ConfigInvalidElementException.
		/// </summary>
		public IDictionary<string, IConfigElement> Elements => throw new ConfigInvalidElementException();
		/// <summary>
		/// Always throws a ConfigInvalidElementException.
		/// </summary>
		public string Key => throw new ConfigInvalidElementException();
		/// <summary>
		/// Always throws a ConfigInvalidElementException.
		/// </summary>
		public string Path => throw new ConfigInvalidElementException();
		/// <summary>
		/// By default, always an empty string. Setting always throws a ConfigInvalidElementException.
		/// You can change the value returned by setting ConfigInvalidElement.InvalidValue.
		/// </summary>
		public string Value { get => InvalidValue; set => throw new ConfigInvalidElementException(); }
		/// <summary>
		/// Always false.
		/// </summary>
		public bool IsValid => false;
		/// <summary>
		/// Always returns Inst.
		/// </summary>
		public IConfigElement GetElement(string key)
		{
			return Inst;
		}
		/// <summary>
		/// Always throws a ConfigInvalidElementException.
		/// </summary>
		public void ThrowIfInvalid()
		{
			throw new ConfigInvalidElementException();
		}
		public ConfigArrayElement AsArrayElement()
		{
			throw new ConfigInvalidElementException();
		}
		public ConfigSectionElement AsSectionElement()
		{
			throw new ConfigInvalidElementException();
		}
		public ConfigStringElement AsStringElement()
		{
			throw new ConfigInvalidElementException();
		}
	}
}

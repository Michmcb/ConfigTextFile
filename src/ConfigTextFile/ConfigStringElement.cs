namespace ConfigTextFile
{
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Represents a single string within the <see cref="ConfigFile"/>.
	/// It does not have any children but it does have a Value.
	/// </summary>
	public class ConfigStringElement : IConfigElement
	{
		/// <summary>
		/// Creates a new instance. Path is set when this is added to a <see cref="ConfigSectionElement"/> or a <see cref="ConfigArrayElement"/>.
		/// Comments are set to null.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="value">The value.</param>
		public ConfigStringElement(string key, string value)
		{
			Path = string.Empty;
			Key = key;
			Value = value;
			Comments = null;
		}
		/// <summary>
		/// Creates a new instance. Path is set when this is added to a <see cref="ConfigSectionElement"/> or a <see cref="ConfigArrayElement"/>.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="value">The value.</param>
		/// <param name="comments">The comments to use.</param>
		public ConfigStringElement(string key, string value, string? comments)
		{
			Path = string.Empty;
			Key = key;
			Value = value;
			Comments = comments;
		}
		/// <summary>
		/// Always throws an <see cref="InvalidOperationException"/>, as strings don't have any children.
		/// </summary>
		/// <param name="key">Always throws whatever the value of this is.</param>
		public string this[string key]
		{
			get => throw new InvalidOperationException("This is a " + nameof(ConfigStringElement) + ", it has no children to get");
			set => throw new InvalidOperationException("This is a " + nameof(ConfigStringElement) + ", it has no children to set");
		}
		/// <summary>
		/// The Key of this <see cref="ConfigStringElement"/>.
		/// </summary>
		public string Key { get; }
		/// <summary>
		/// The full path to this <see cref="ConfigStringElement"/>.
		/// This is set once this element is added as a child to another element.
		/// </summary>
		public string Path { get; internal set; }
		/// <summary>
		/// Gets or sets this <see cref="ConfigStringElement"/>'s value.
		/// </summary>
		public string Value { get; set; }
		/// <summary>
		/// Returns <see cref="Value"/> because this is a <see cref="ConfigStringElement"/>.
		/// </summary>
		/// <param name="alternative">This is never returned</param>
		/// <returns><see cref="Value"/></returns>
		[return: NotNullIfNotNull("alternative")]
		public string? ValueOr(string? alternative)
		{
			return Value;
		}
		/// <summary>
		/// Always true.
		/// </summary>
		public bool IsValid => true;
		/// <summary>
		/// Returns <see cref="ConfigElementType.String"/>.
		/// </summary>
		public ConfigElementType Type => ConfigElementType.String;
		/// <summary>
		/// The comments that preceded this <see cref="ConfigStringElement"/>.
		/// </summary>
		public string? Comments { get; set; }
		/// <summary>
		/// Always throws an <see cref="InvalidOperationException"/>.
		/// </summary>
		public IConfigElement GetElement(string key)
		{
			throw new InvalidOperationException("This is a " + nameof(ConfigStringElement) + ", it has no children to get");
		}
		/// <summary>
		/// Always returns <see cref="ConfigInvalidElement.Inst"/>.
		/// </summary>
		public IConfigElement TryGetElement(string key)
		{
			return ConfigInvalidElement.Inst;
		}
		/// <summary>
		/// Always throws <see cref="InvalidOperationException"/>.
		/// </summary>
		public IConfigElement FindElement(string path)
		{
			throw new InvalidOperationException("This is a " + nameof(ConfigStringElement) + ", it has no children to get");
		}
		/// <summary>	
		/// Always returns <see cref="ConfigInvalidElement.Inst"/>.
		/// </summary>
		public IConfigElement TryFindElement(string path)
		{
			return ConfigInvalidElement.Inst;
		}
		/// <summary>
		/// Returns <see cref="Path"/>=<see cref="Value"/>. If <see cref="Value"/> is null, it's treated as <see cref="string.Empty"/>.
		/// </summary>
		public override string ToString()
		{
			return string.Concat(Path, "=", Value);
		}
		/// <summary>
		/// Never throws.
		/// </summary>
		public void ThrowIfInvalid() { }
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		/// <returns>Always throws.</returns>
		public ConfigArrayElement AsArrayElement()
		{
			throw new InvalidCastException("This is not a " + nameof(ConfigArrayElement) + "; it is a " + nameof(ConfigStringElement));
		}
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		/// <returns>Always throws.</returns>
		public ConfigSectionElement AsSectionElement()
		{
			throw new InvalidCastException("This is not a " + nameof(ConfigSectionElement) + "; it is a " + nameof(ConfigStringElement));
		}
		/// <summary>
		/// Returns this.
		/// </summary>
		/// <returns>This.</returns>
		public ConfigStringElement AsStringElement()
		{
			return this;
		}
	}
}

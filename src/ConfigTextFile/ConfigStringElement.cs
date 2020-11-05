namespace ConfigTextFile
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents a single string within the <see cref="ConfigFile"/>.
	/// It does not have any children but it does have a Value.
	/// </summary>
	public partial class ConfigStringElement : IConfigElement
	{
		/// <summary>
		/// Creates a new <see cref="ConfigStringElement"/>, with an empty list of comments.
		/// </summary>
		public ConfigStringElement(string key, string path, string value)
		{
			Key = key;
			Path = path;
			Value = value;
			Comments = new List<string>();
		}
		/// <summary>
		/// Creates a new <see cref="ConfigStringElement"/>, with a new list of comments created from <paramref name="comments"/>.
		/// </summary>
		public ConfigStringElement(string key, string path, string value, params string[] comments)
		{
			Key = key;
			Path = path;
			Value = value;
			Comments = new List<string>(comments);
		}
		/// <summary>
		/// Creates a new <see cref="ConfigStringElement"/>, with <paramref name="comments"/> used directly (it is not copied).
		/// </summary>
		public ConfigStringElement(string key, string path, string value, ICollection<string> comments)
		{
			Key = key;
			Path = path;
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
		/// </summary>
		public string Path { get; }
		/// <summary>
		/// Gets or sets this <see cref="ConfigStringElement"/>'s value.
		/// </summary>
		public string Value { get; set; }
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
		public ICollection<string> Comments { get; set; }
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
		/// <summary>
		/// If <see cref="Value"/> is null or empty, returns <paramref name="alternate"/>. Otherwise, returns <see cref="Value"/>.
		/// </summary>
		/// <param name="alternate">The alternate value to return.</param>
		/// <returns><see cref="Value"/> if it's not null/empty, or <paramref name="alternate"/> if it is.</returns>
		public string ValueOr(string alternate)
		{
			return string.IsNullOrEmpty(Value) ? alternate : Value;
		}
	}
}

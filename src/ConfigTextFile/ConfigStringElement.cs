namespace ConfigTextFile
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents a single string within the ConfigFile.
	/// It does not have any children but it does have a Value.
	/// </summary>
	public class ConfigStringElement : IConfigElement
	{
		/// <summary>
		/// Creates a new <see cref="ConfigArrayElement"/>, with the provided <paramref name="key"/>, <paramref name="path"/>, <paramref name="value"/>, and no comments.
		/// </summary>
		public ConfigStringElement(string key, string path, string value)
		{
			Key = key;
			Path = path;
			Value = value;
			Comments = new List<string>();
		}
		/// <summary>
		/// Creates a new <see cref="ConfigArrayElement"/>, with the provided <paramref name="key"/>, <paramref name="path"/>, <paramref name="value"/>, and <paramref name="comments"/>.
		/// </summary>
		public ConfigStringElement(string key, string path, string value, params string[] comments)
		{
			Key = key;
			Path = path;
			Value = value;
			Comments = new List<string>(comments);
		}
		/// <summary>
		/// Creates a new <see cref="ConfigArrayElement"/>, with the provided <paramref name="key"/>, <paramref name="path"/>, <paramref name="value"/>, and <paramref name="comments"/>.
		/// </summary>
		public ConfigStringElement(string key, string path, string value, IEnumerable<string> comments)
		{
			Key = key;
			Path = path;
			Value = value;
			Comments = new List<string>(comments);
		}
		/// <summary>
		/// Always throws an InvalidOperationException, as strings don't have any children.
		/// </summary>
		/// <param name="key">Doesn't matter, always throws.</param>
		public string this[string key]
		{
			get
			{
				throw new InvalidOperationException("This is a ConfigStringElement; it has no children to get");
			}
			set
			{
				throw new InvalidOperationException("This is a ConfigStringElement; it has no children to set");
			}
		}
		/// <summary>
		/// The Key of this ConfigElement.
		/// </summary>
		public string Key { get; }
		/// <summary>
		/// The full path to this ConfigElement.
		/// </summary>
		public string Path { get; }
		/// <summary>
		/// Gets or sets this ConfigElement's value.
		/// </summary>
		public string Value { get; set; }
		/// <summary>
		/// Always true.
		/// </summary>
		public bool IsValid => true;
		/// <summary>
		/// Always throws an InvalidOperationException, as strings don't have any children.
		/// </summary>
		public IDictionary<string, IConfigElement> Elements => throw new InvalidOperationException("This is a ConfigStringElement; it has no children to get");
		/// <summary>
		/// Returns ConfigElementType.String.
		/// </summary>
		public ConfigElementType Type => ConfigElementType.String;
		/// <summary>
		/// The comments that preceded this ConfigStringElement.
		/// </summary>
		public ICollection<string> Comments { get; set; }
		/// <summary>
		/// Returns the ConfigInvalidElement instance.
		/// </summary>
		public IConfigElement GetElement(string key)
		{
			return ConfigInvalidElement.Inst;
		}
		/// <summary>
		/// Returns Path=Value.
		/// </summary>
		public override string ToString()
		{
			return string.Concat(Path, "=", Value ?? "(null)");
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
			throw new InvalidCastException("This is not a ConfigArrayElement; it is a ConfigStringElement");
		}
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		/// <returns>Always throws.</returns>
		public ConfigSectionElement AsSectionElement()
		{
			throw new InvalidCastException("This is not a ConfigSectionElement; it is a ConfigStringElement");
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

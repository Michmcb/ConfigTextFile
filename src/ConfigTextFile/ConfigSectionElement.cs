namespace ConfigTextFile
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents a section within the <see cref="ConfigFile"/>.
	/// It has children but cannot have any Value set.
	/// </summary>
	public class ConfigSectionElement : IConfigElement
	{
		/// <summary>
		/// Creates a new <see cref="ConfigSectionElement"/>, with no comments.
		/// </summary>
		public ConfigSectionElement(string key, string path)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
			Comments = new List<string>();
		}
		/// <summary>
		/// Creates a new <see cref="ConfigSectionElement"/>, with <paramref name="comments"/>.
		/// </summary>
		public ConfigSectionElement(string key, string path, params string[] comments)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
			Comments = new List<string>(comments);
		}
		/// <summary>
		/// Creates a new <see cref="ConfigSectionElement"/>, with <paramref name="comments"/>.
		/// </summary>
		public ConfigSectionElement(string key, string path, IEnumerable<string> comments)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
			Comments = new List<string>(comments);
		}
		/// <summary>
		/// Gets or sets a child element's value.
		/// <paramref name="key"/> should refer to a <see cref="ConfigStringElement"/>.
		/// If the key was not found, throws a <see cref="KeyNotFoundException"/>.
		/// If the key was found but it wasn't a <see cref="ConfigStringElement"/>, throws an <see cref="InvalidOperationException"/>.
		/// </summary>
		/// <param name="key">Gets or sets an <see cref="IConfigElement"/>'s Value whose Key property matches this.</param>
		public string this[string key]
		{
			get
			{
				return Elements.TryGetValue(key, out IConfigElement elem)
					? elem.Value
					: throw new KeyNotFoundException("There is no " + nameof(ConfigStringElement) + " with the key " + key);
			}
			set
			{
				if (Elements.TryGetValue(key, out IConfigElement elem))
				{
					elem.Value = value;
				}
				else
				{
					throw new KeyNotFoundException("There is no " + nameof(ConfigStringElement) + " with the key " + key);
				}
			}
		}
		/// <summary>
		/// The Key of this <see cref="ConfigSectionElement"/>.
		/// </summary>
		public string Key { get; }
		/// <summary>
		/// The full path to this <see cref="ConfigSectionElement"/>.
		/// </summary>
		public string Path { get; }
		/// <summary>
		/// Always throws <see cref="InvalidOperationException"/> on setting. Always returns <see cref="string.Empty"/>.
		/// </summary>
		public string Value
		{
			get => string.Empty;
			set => throw new InvalidOperationException("You cannot set the value of a " + nameof(ConfigSectionElement));
		}
		/// <summary>
		/// Always true.
		/// </summary>
		public bool IsValid => true;
		/// <summary>
		/// All <see cref="IConfigElement"/> within this Section.
		/// </summary>
		public IDictionary<string, IConfigElement> Elements { get; }
		/// <summary>
		/// Returns <see cref="ConfigElementType.Section"/>.
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Section;
		/// <summary>
		/// The comments that preceded this <see cref="ConfigSectionElement"/>.
		/// </summary>
		public ICollection<string> Comments { get; set; }
		/// <summary>
		/// Tries to get the <see cref="IConfigElement"/> identified by <paramref name="key"/>.
		/// If it does not exist, throws a <see cref="KeyNotFoundException"/>.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		public IConfigElement GetElement(string key)
		{
			return Elements.TryGetValue(key, out IConfigElement section) ? section : throw new KeyNotFoundException("There is no " + nameof(IConfigElement) + " with the key " + key);
		}
		/// <summary>
		/// Tries to get the <see cref="IConfigElement"/> identified by <paramref name="key"/>.
		/// If it does not exist, returns <see cref="ConfigInvalidElement.Inst"/>.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		public IConfigElement TryGetElement(string key)
		{
			return Elements.TryGetValue(key, out IConfigElement section) ? section : ConfigInvalidElement.Inst;
		}
		/// <summary>
		/// Returns <see cref="Path"/>.
		/// </summary>
		public override string ToString()
		{
			return Path;
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
			throw new InvalidCastException("This is not a " + nameof(ConfigArrayElement) + "; it is a " + nameof(ConfigSectionElement));
		}
		/// <summary>
		/// Returns this.
		/// </summary>
		/// <returns>This.</returns>
		public ConfigSectionElement AsSectionElement()
		{
			return this;
		}
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		/// <returns>Always throws.</returns>
		public ConfigStringElement AsStringElement()
		{
			throw new InvalidCastException("This is not a " + nameof(ConfigStringElement) + "; it is a " + nameof(ConfigSectionElement));
		}
	}
}

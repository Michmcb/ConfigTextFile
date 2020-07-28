namespace ConfigTextFile
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents a section within the ConfigFile.
	/// It has children but cannot have any Value set.
	/// </summary>
	public class ConfigSectionElement : IConfigElement
	{
		/// <summary>
		/// Creates a new <see cref="ConfigSectionElement"/>, with the provided <paramref name="key"/> and <paramref name="path"/>, and no comments.
		/// </summary>
		public ConfigSectionElement(string key, string path)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
		}
		/// <summary>
		/// Creates a new <see cref="ConfigSectionElement"/>, with the provided <paramref name="key"/> and <paramref name="path"/>, with the provided <paramref name="comments"/>.
		/// </summary>
		public ConfigSectionElement(string key, string path, params string[] comments)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
			Comments = new List<string>(comments);
		}
		/// <summary>
		/// Creates a new <see cref="ConfigSectionElement"/>, with the provided <paramref name="key"/> and <paramref name="path"/>, with the provided <paramref name="comments"/>.
		/// </summary>
		public ConfigSectionElement(string key, string path, IEnumerable<string> comments)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
			Comments = new List<string>(comments);
		}
		/// <summary>
		/// Gets or sets a IConfigElement's value.
		/// <paramref name="key"/> should refer to a ConfigStringElement.
		/// </summary>
		/// <param name="key">Gets an IConfigElement whose Key property matches this.</param>
		public string this[string key]
		{
			get
			{
				return Elements.TryGetValue(key, out IConfigElement section)
					? section.Type == ConfigElementType.String
						? section.Value
						: throw new InvalidOperationException(string.Concat("The ConfigElement with the key ", key, "was found, but it was a " + section.Type + ", not a String. Path: ", section.Path))
					: throw new KeyNotFoundException("There is no ConfigStringElement with the key " + key);
			}
			set
			{
				if (Elements.TryGetValue(key, out IConfigElement section))
				{
					section.Value = value;
				}
				else
				{
					throw new KeyNotFoundException("There is no ConfigElement with the key " + key);
				}
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
		/// Always returns an empty string. Setting this throws an InvalidOperationException.
		/// </summary>
		public string Value { get => string.Empty; set => throw new InvalidOperationException("You cannot set the value of a ConfigSectionElement"); }
		/// <summary>
		/// Always true.
		/// </summary>
		public bool IsValid => true;
		/// <summary>
		/// All IConfigElements within this Section.
		/// </summary>
		public IDictionary<string, IConfigElement> Elements { get; }
		/// <summary>
		/// Returns ConfigElementType.Section.
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Section;
		/// <summary>
		/// The comments that preceded this ConfigSectionElement.
		/// </summary>
		public ICollection<string> Comments { get; set; }
		/// <summary>
		/// Tries to get the ConfigElement identified by <paramref name="key"/>.
		/// If it does not exist, returns a <see cref="ConfigInvalidElement"/>.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		public IConfigElement GetElement(string key)
		{
			return Elements.TryGetValue(key, out IConfigElement section) ? section : ConfigInvalidElement.Inst;
		}
		/// <summary>
		/// Returns Path.
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
			throw new InvalidCastException("This is not a ConfigArrayElement; it is a ConfigSectionElement");
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
			throw new InvalidCastException("This is not a ConfigStringElement; it is a ConfigSectionElement");
		}
	}
}

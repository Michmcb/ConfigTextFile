namespace ConfigTextFile
{
	using ConfigTextFile.IO;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents a section within the <see cref="ConfigFile"/>.
	/// It has children but cannot have any Value set.
	/// </summary>
	public class ConfigSectionElement : IConfigElement
	{
		/// <summary>
		/// Creates a new <see cref="ConfigSectionElement"/>, with an empty list of comments.
		/// </summary>
		public ConfigSectionElement(string key, string path)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
			Comments = new List<string>();
		}
		/// <summary>
		/// Creates a new <see cref="ConfigSectionElement"/>, with a new list of comments created from <paramref name="comments"/>.
		/// </summary>
		public ConfigSectionElement(string key, string path, params string[] comments)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
			Comments = new List<string>(comments);
		}
		/// <summary>
		/// Creates a new <see cref="ConfigSectionElement"/>, with <paramref name="comments"/> used directly (it is not copied).
		/// </summary>
		public ConfigSectionElement(string key, string path, ICollection<string> comments)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
			Comments = comments;
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
				return Elements.TryGetValue(key, out IConfigElement? elem)
					? elem.Value
					: throw new KeyNotFoundException("There is no " + nameof(ConfigStringElement) + " with the key " + key);
			}
			set
			{
				if (Elements.TryGetValue(key, out IConfigElement? elem))
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
			return Elements.TryGetValue(key, out IConfigElement? section) ? section : throw new KeyNotFoundException("There is no " + nameof(IConfigElement) + " with the key " + key);
		}
		/// <summary>
		/// Tries to get the <see cref="IConfigElement"/> identified by <paramref name="key"/>.
		/// If it does not exist, returns <see cref="ConfigInvalidElement.Inst"/>.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		public IConfigElement TryGetElement(string key)
		{
			return Elements.TryGetValue(key, out IConfigElement? element) ? element : ConfigInvalidElement.Inst;
		}
		/// <summary>
		/// Attempts to find an <see cref="IConfigElement"/> which has the path <paramref name="path"/>. Goes as deep as it needs to to find one.
		/// If none is found, throws a <see cref="KeyNotFoundException"/>.
		/// </summary>
		/// <param name="path">The path of the element to search for.</param>
		public IConfigElement FindElement(string path)
		{
			IConfigElement e = TryFindElement(path);
			return e.IsValid
					 ? e
					 : throw new KeyNotFoundException(string.Concat("Could not find any ", nameof(IConfigElement), " with the path ", path));
		}
		/// <summary>
		/// Attempts to find an <see cref="IConfigElement"/> which has the path <paramref name="path"/>. Goes as deep as it needs to to find one.
		/// If none is found, returns a <see cref="ConfigInvalidElement"/>.
		/// </summary>
		/// <param name="path">The path of the element to search for.</param>
		public IConfigElement TryFindElement(string path)
		{
			// Find the delimiter first
			int i = path.IndexOf(SyntaxCharacters.SectionDelimiter);
			if (i == -1)
			{
				// If none is found, it is probably the end of the path, so just return that
				return Elements.TryGetValue(path, out IConfigElement? element) ? element : ConfigInvalidElement.Inst;
			}
			// If there IS a delimiter, we'll take a substring and keep trying to find the element
			// But if the string happens to end with a delimiter e.g. "section:", then that's wrong and would also make our next substring fail.
			// So check that before we make a substring
			else if (path.Length != i + 1 && Elements.TryGetValue(path.Substring(0, i), out IConfigElement? element))
			{
				return element.TryFindElement(path.Substring(i + 1));
			}
			return ConfigInvalidElement.Inst;
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

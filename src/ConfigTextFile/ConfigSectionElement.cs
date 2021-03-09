namespace ConfigTextFile
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// Represents a section within the <see cref="ConfigFile"/>.
	/// It has children but cannot have any Value set.
	/// </summary>
	public class ConfigSectionElement : IConfigElement
	{
		internal readonly Dictionary<string, IConfigElement> _elements;
		/// <summary>
		/// Creates a new instance, whose key/path are <see cref="string.Empty"/>, and comments are <see cref="Array.Empty{T}"/>.
		/// This can be used as an argument to create a new <see cref="ConfigFile"/>, if needed.
		/// </summary>
		/// <param name="keyComparer">The comparer to use when comparing keys for elements added to this.</param>
		/// <returns>A <see cref="ConfigSectionElement"/> which can be used as a root.</returns>
		public ConfigSectionElement(IEqualityComparer<string> keyComparer)
		{
			Path = string.Empty;
			Key = string.Empty;
			_elements = new Dictionary<string, IConfigElement>(keyComparer);
			Comments = string.Empty;
		}
		/// <summary>
		/// Creates a new instance. Path is set when this is added to a <see cref="ConfigSectionElement"/>.
		/// Comments are set to a new empty list. Value is set to <see cref="string.Empty"/>.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="keyComparer">The comparer to use when comparing keys for elements added to this.</param>
		public ConfigSectionElement(string key, IEqualityComparer<string> keyComparer)
		{
			Path = string.Empty;
			Key = key;
			_elements = new Dictionary<string, IConfigElement>(keyComparer);
			Comments = string.Empty;
		}
		/// <summary>
		/// Creates a new instance. Path is set when this is added to a <see cref="ConfigSectionElement"/>.
		/// Comments are set to . Value is set to <see cref="string.Empty"/>.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="keyComparer">The comparer to use when comparing keys for elements added to this.</param>
		/// <param name="comments">The comments to use.</param>
		public ConfigSectionElement(string key, IEqualityComparer<string> keyComparer, string? comments)
		{
			Path = string.Empty;
			Key = key;
			_elements = new Dictionary<string, IConfigElement>(keyComparer);
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
		public string Path { get; internal set; }
		/// <summary>
		/// Always throws <see cref="InvalidOperationException"/> on setting. Always returns <see cref="string.Empty"/>.
		/// </summary>
		public string Value
		{
			get => string.Empty;
			set => throw new InvalidOperationException("You cannot set the value of a " + nameof(ConfigSectionElement));
		}
		/// <summary>
		/// Returns <paramref name="alternative"/> because this is a <see cref="ConfigSectionElement"/>.
		/// </summary>
		/// <param name="alternative">Returns this.</param>
		/// <returns><paramref name="alternative"/></returns>
		[return: NotNullIfNotNull("alternative")]
		public string? ValueOr(string? alternative)
		{
			return alternative;
		}
		/// <summary>
		/// Always true.
		/// </summary>
		public bool IsValid => true;
		/// <summary>
		/// All <see cref="IConfigElement"/> within this Section.
		/// </summary>
		public IReadOnlyDictionary<string, IConfigElement> Elements => _elements;
		/// <summary>
		/// Returns <see cref="ConfigElementType.Section"/>.
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Section;
		/// <summary>
		/// The comments that preceded this.
		/// </summary>
		public string? Comments { get; set; }
		/// <summary>
		/// The <see cref="IEqualityComparer{T}"/> used to compare the keys of elements added to this.
		/// </summary>
		public IEqualityComparer<string> KeyComparer => _elements.Comparer;
		/// <summary>
		/// Enumerates over all elements that this section contains. Does not filter by type.
		/// </summary>
		/// <returns>All <see cref="IConfigElement"/> in this section.</returns>
		public IEnumerable<IConfigElement> EnumerateElements()
		{
			return _elements.Values;
		}
		/// <summary>
		/// Enumerates over all <see cref="ConfigStringElement"/> elements that this section contains.
		/// </summary>
		/// <returns>All <see cref="ConfigStringElement"/> in this section.</returns>
		public IEnumerable<ConfigStringElement> EnumerateStringElements()
		{
			foreach (IConfigElement elem in _elements.Values)
			{
				if (elem.Type == ConfigElementType.String) yield return elem.AsStringElement();
			}
		}
		/// <summary>
		/// Enumerates over all <see cref="ConfigSectionElement"/> elements that this section contains.
		/// </summary>
		/// <returns>All <see cref="ConfigSectionElement"/> in this section.</returns>
		public IEnumerable<ConfigSectionElement> EnumerateSectionElements()
		{
			foreach (IConfigElement elem in _elements.Values)
			{
				if (elem.Type == ConfigElementType.Section) yield return elem.AsSectionElement();
			}
		}
		/// <summary>
		/// Enumerates over all <see cref="ConfigArrayElement"/> elements that this section contains.
		/// </summary>
		/// <returns>All <see cref="ConfigArrayElement"/> in this section.</returns>
		public IEnumerable<ConfigArrayElement> EnumerateArrayElements()
		{
			foreach (IConfigElement elem in _elements.Values)
			{
				if (elem.Type == ConfigElementType.Array) yield return elem.AsArrayElement();
			}
		}
		/// <summary>
		/// Adds <paramref name="element"/> to <see cref="Elements"/>. Sets the Path of <paramref name="element"/> if successful.
		/// If <paramref name="element"/> has already been added to something else, or the Key is already taken, throws a <see cref="ArgumentException"/>.
		/// </summary>
		/// <param name="element">The element to add</param>
		public void AddElement(ConfigStringElement element)
		{
			if (element.Path.Length == 0)
			{
				if (!_elements.ContainsKey(element.Key))
				{
					_elements.Add(element.Key, element);
					element.Path = ConfigPath.Join(Path, element.Key);
					return;
				}
				throw new ArgumentException("An element with the same key has already been added", nameof(element));
			}
			throw new ArgumentException("The provided element has already been added to something else; its path is " + element.Path, nameof(element));
		}
		/// <summary>
		/// Adds <paramref name="element"/> to <see cref="Elements"/>. Sets the Path of <paramref name="element"/> if successful.
		/// If <paramref name="element"/> has already been added to something else, or the Key is already taken, returns false.
		/// </summary>
		/// <param name="element">The element to add</param>
		/// <returns>true if <paramref name="element"/> was added, false otherwise.</returns>
		public bool TryAddElement(ConfigStringElement element)
		{
			if (element.Path.Length == 0 && !_elements.ContainsKey(element.Key))
			{
				_elements.Add(element.Key, element);
				element.Path = ConfigPath.Join(Path, element.Key);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Adds <paramref name="element"/> to <see cref="Elements"/>. Sets the Path of <paramref name="element"/> if successful.
		/// If <paramref name="element"/> has already been added to something else, or the Key is already taken, throws a <see cref="ArgumentException"/>.
		/// </summary>
		/// <param name="element">The element to add</param>
		public void AddElement(ConfigArrayElement element)
		{
			if (element.Path.Length == 0)
			{
				if (!_elements.ContainsKey(element.Key))
				{
					_elements.Add(element.Key, element);
					element.Path = ConfigPath.Join(Path, element.Key);
					return;
				}
				throw new ArgumentException("An element with the same key has already been added", nameof(element));
			}
			throw new ArgumentException("The provided element has already been added to something else; its path is " + element.Path, nameof(element));
		}
		/// <summary>
		/// Adds <paramref name="element"/> to <see cref="Elements"/>. Sets the Path of <paramref name="element"/> if successful.
		/// If <paramref name="element"/> has already been added to something else, or the Key is already taken, returns false.
		/// </summary>
		/// <param name="element">The element to add</param>
		/// <returns>true if <paramref name="element"/> was added, false otherwise.</returns>
		public bool TryAddElement(ConfigArrayElement element)
		{
			if (element.Path.Length == 0 && !_elements.ContainsKey(element.Key))
			{
				_elements.Add(element.Key, element);
				element.Path = ConfigPath.Join(Path, element.Key);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Adds <paramref name="element"/> to <see cref="Elements"/>. Sets the Path of <paramref name="element"/> if successful.
		/// If <paramref name="element"/> has already been added to something else, or the Key is already taken, throws a <see cref="ArgumentException"/>.
		/// </summary>
		/// <param name="element">The element to add</param>
		public void AddElement(ConfigSectionElement element)
		{
			if (element.Path.Length == 0)
			{
				if (!_elements.ContainsKey(element.Key))
				{
					_elements.Add(element.Key, element);
					element.Path = ConfigPath.Join(Path, element.Key);
					return;
				}
				throw new ArgumentException("An element with the same key has already been added", nameof(element));
			}
			throw new ArgumentException("The provided element has already been added to something else; its path is " + element.Path, nameof(element));
		}
		/// <summary>
		/// Adds <paramref name="element"/> to <see cref="Elements"/>. Sets the Path of <paramref name="element"/> if successful.
		/// If <paramref name="element"/> has already been added to something else, or the Key is already taken, returns false.
		/// </summary>
		/// <param name="element">The element to add</param>
		/// <returns>true if <paramref name="element"/> was added, false otherwise.</returns>
		public bool TryAddElement(ConfigSectionElement element)
		{
			if (element.Path.Length == 0 && !_elements.ContainsKey(element.Key))
			{
				_elements.Add(element.Key, element);
				element.Path = ConfigPath.Join(Path, element.Key);
				return true;
			}
			return false;
		}
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
			int i = path.IndexOf(ConfigPath.Separator);
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

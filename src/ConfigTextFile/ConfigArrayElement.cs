namespace ConfigTextFile
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Represents an array of strings within the <see cref="ConfigFile"/>.
	/// It has children but cannot have any Value set.
	/// </summary>
	public class ConfigArrayElement : IConfigElement
	{
		private readonly List<ConfigStringElement> _elements;
		/// <summary>
		/// Creates a new instance. Path is set when this is added to a <see cref="ConfigSectionElement"/>.
		/// Comments are set to a new empty list. Value is set to <see cref="string.Empty"/>.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="values">The values; creates a new <see cref="ConfigStringElement"/> for every string in this collection.</param>
		public ConfigArrayElement(string key, IEnumerable<string> values)
		{
			Path = string.Empty;
			Key = key;
			_elements = new List<ConfigStringElement>();
			foreach (string val in values)
			{
				AddNewString(val);
			}
			Comments = new List<string>();
		}
		/// <summary>
		/// Creates a new instance. Path is set when this is added to a <see cref="ConfigSectionElement"/>.
		/// Comments are set to a new empty list. Value is set to <see cref="string.Empty"/>.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="values">The values; creates a new <see cref="ConfigStringElement"/> for every string in this collection.</param>
		public ConfigArrayElement(string key, params string[] values)
		{
			Path = string.Empty;
			Key = key;
			_elements = new List<ConfigStringElement>();
			foreach (string val in values)
			{
				AddNewString(val);
			}
			Comments = new List<string>();
		}
		/// <summary>
		/// Creates a new instance. Path is set when this is added to a <see cref="ConfigSectionElement"/>.
		/// Comments are set to . Value is set to <see cref="string.Empty"/>.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="values">The values; creates a new <see cref="ConfigStringElement"/> for every string in this collection.</param>
		/// <param name="comments">The comments to use. If <paramref name="copyComments"/> is true they are copied, otherwise they are used directly.</param>
		/// <param name="copyComments">If true, copies <paramref name="comments"/> into a new list. Otherwise, assigns directly.</param>
		public ConfigArrayElement(string key, IEnumerable<string> values, ICollection<string> comments, bool copyComments = true)
		{
			Path = string.Empty;
			Key = key;
			_elements = new List<ConfigStringElement>();
			foreach (string val in values)
			{
				AddNewString(val);
			}
			Comments = copyComments ? new List<string>(comments) : comments;
		}
		/// <summary>
		/// Gets or sets a child element's value.
		/// If the key was not found, throws a <see cref="KeyNotFoundException"/>.
		/// </summary>
		/// <param name="key">Gets or sets an <see cref="ConfigStringElement"/>'s Value whose Key property matches this.</param>
		public string this[string key]
		{
			get
			{
				IConfigElement e = TryGetElement(key);
				return e.IsValid
					? e.Value
					: throw new KeyNotFoundException(string.Concat("There is no ", nameof(ConfigStringElement), " with a key of ", key, " or the key cannot be parsed as an int as this is a ", nameof(ConfigArrayElement)));
			}
			set
			{
				IConfigElement e = TryGetElement(key);
				e.Value = e.IsValid
					? value
					: throw new KeyNotFoundException(string.Concat("There is no ", nameof(ConfigStringElement), " with a key of ", key, " or the key cannot be parsed as an int as this is a ", nameof(ConfigArrayElement)));
			}
		}
		/// <summary>
		/// Gets or sets a child element's value by index. Use this when you can; the one that accepts strings as keys has to parse them as an int.
		/// Throws an <see cref="IndexOutOfRangeException"/> if <paramref name="index"/> falls outside the range of <see cref="Elements"/>.
		/// </summary>
		/// <param name="index">Gets or sets an <see cref="ConfigStringElement"/>'s Value.</param>
		public string this[int index]
		{
			get => Elements[index].Value;
			set => Elements[index].Value = value;
		}
		/// <summary>
		/// The Key of this <see cref="ConfigArrayElement"/>.
		/// </summary>
		public string Key { get; }
		/// <summary>
		/// The full path to this <see cref="ConfigArrayElement"/>.
		/// </summary>
		public string Path { get; internal set; }
		/// <summary>
		/// Always throws <see cref="InvalidOperationException"/> on setting. Always returns <see cref="string.Empty"/>.
		/// </summary>
		public string Value
		{
			get => string.Empty;
			set => throw new InvalidOperationException("You cannot set the value of a " + nameof(ConfigArrayElement));
		}
		/// <summary>
		/// Always true.
		/// </summary>
		public bool IsValid => true;
		/// <summary>
		/// All <see cref="ConfigStringElement"/>s within this Array.
		/// </summary>
		public IReadOnlyList<ConfigStringElement> Elements => _elements;
		/// <summary>
		/// Returns <see cref="ConfigElementType.Array"/>.
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Array;
		/// <summary>
		/// The comments that preceded this <see cref="ConfigArrayElement"/>.
		/// </summary>
		public ICollection<string> Comments { get; set; }
		/// <summary>
		/// Creates a new <see cref="ConfigStringElement"/>, and adds it to <see cref="Elements"/>.
		/// </summary>
		/// <param name="value">The value. If null, will be set to <see cref="string.Empty"/> instead.</param>
		/// <returns>A new <see cref="ConfigStringElement"/></returns>
		public ConfigStringElement AddNewString(string value)
		{
			ConfigStringElement e = new ConfigStringElement(_elements.Count.ToString(), value ?? string.Empty, Array.Empty<string>(), copyComments: false);
			e.Path = ConfigPath.Join(Path, e.Key);
			_elements.Add(e);
			return e;
		}
		/// <summary>
		/// Tries to get the <see cref="IConfigElement"/> identified by <paramref name="key"/>, which must be able to be parsed as an int.
		/// If it does not exist, or <paramref name="key"/> parses to an int which is outside the range of <see cref="Elements"/>, throws a <see cref="KeyNotFoundException"/>.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		public IConfigElement GetElement(string key)
		{
			return int.TryParse(key, out int index) && index >= 0 && Elements.Count > index
				? Elements[index]
				: throw new KeyNotFoundException(string.Concat("There is no ", nameof(ConfigStringElement), " with a key of ", key, " or the key cannot be parsed as an int as this is a ", nameof(ConfigArrayElement)));
		}
		/// <summary>
		/// Tries to get the <see cref="IConfigElement"/> identified by <paramref name="key"/>, which must be able to be parsed as an int.
		/// If it does not exist, or <paramref name="key"/> parses to an int which is outside the range of <see cref="Elements"/>, returns <see cref="ConfigInvalidElement.Inst"/>.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		public IConfigElement TryGetElement(string key)
		{
			return int.TryParse(key, out int index) && index >= 0 && Elements.Count > index
				? Elements[index]
				: (IConfigElement)ConfigInvalidElement.Inst;
		}
		/// <summary>
		/// Attempts to find an <see cref="IConfigElement"/> which has the path <paramref name="path"/>. Goes as deep as it needs to to find one.
		/// If none is found, throws a <see cref="KeyNotFoundException"/>.
		/// </summary>
		/// <param name="path">The path of the element to search for.</param>
		public IConfigElement FindElement(string path)
		{
			return GetElement(path);
		}
		/// <summary>
		/// Attempts to find an <see cref="IConfigElement"/> which has the path <paramref name="path"/>. Goes as deep as it needs to to find one.
		/// If none is found, returns a <see cref="ConfigInvalidElement"/>.
		/// </summary>
		/// <param name="path">The path of the element to search for.</param>
		public IConfigElement TryFindElement(string path)
		{
			return TryGetElement(path);
		}
		/// <summary>
		/// A convenience method that loops over all strings in this array.
		/// </summary>
		public IEnumerable<string> GetValues()
		{
			foreach (ConfigStringElement e in Elements)
			{
				yield return e.Value;
			}
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
		/// Returns this.
		/// </summary>
		/// <returns>This.</returns>
		public ConfigArrayElement AsArrayElement()
		{
			return this;
		}
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		/// <returns>Always throws.</returns>
		public ConfigSectionElement AsSectionElement()
		{
			throw new InvalidCastException("This is not a " + nameof(ConfigSectionElement) + "; it is a " + nameof(ConfigArrayElement));
		}
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		/// <returns>Always throws.</returns>
		public ConfigStringElement AsStringElement()
		{
			throw new InvalidCastException("This is not a " + nameof(ConfigStringElement) + "; it is a " + nameof(ConfigArrayElement));
		}
		internal void AddWithoutCheck(ConfigStringElement element)
		{
			_elements.Add(element);
		}
	}
}

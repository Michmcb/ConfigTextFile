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
		/// <summary>
		/// Creates a new <see cref="ConfigArrayElement"/>, with an empty list of comments.
		/// </summary>
		public ConfigArrayElement(string key, string path)
		{
			Key = key;
			Path = path;
			Elements = new List<ConfigStringElement>();
			Comments = new List<string>();
		}
		/// <summary>
		/// Creates a new <see cref="ConfigArrayElement"/>, with <paramref name="comments"/>, with a new list of comments created from <paramref name="comments"/>.
		/// </summary>
		public ConfigArrayElement(string key, string path, params string[] comments)
		{
			Key = key;
			Path = path;
			Elements = new List<ConfigStringElement>();
			Comments = new List<string>(comments);
		}
		/// <summary>
		/// Creates a new <see cref="ConfigArrayElement"/>, with <paramref name="comments"/> used directly (it is not copied).
		/// </summary>
		public ConfigArrayElement(string key, string path, ICollection<string> comments)
		{
			Key = key;
			Path = path;
			Elements = new List<ConfigStringElement>();
			Comments = comments;
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
				if (e.IsValid)
				{
					return e.Value;
				}
				else
				{
					throw new KeyNotFoundException(string.Concat("There is no ", nameof(ConfigStringElement), " with a key of ", key, " or the key cannot be parsed as an int as this is a ", nameof(ConfigArrayElement)));
				}
			}
			set
			{
				IConfigElement e = TryGetElement(key);
				if (e.IsValid)
				{
					e.Value = value;
				}
				else
				{
					throw new KeyNotFoundException(string.Concat("There is no ", nameof(ConfigStringElement), " with a key of ", key, " or the key cannot be parsed as an int as this is a ", nameof(ConfigArrayElement)));
				}
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
		public string Path { get; }
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
		public IList<ConfigStringElement> Elements { get; }
		/// <summary>
		/// Returns <see cref="ConfigElementType.Array"/>.
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Array;
		/// <summary>
		/// The comments that preceded this <see cref="ConfigArrayElement"/>.
		/// </summary>
		public ICollection<string> Comments { get; set; }
		/// <summary>
		/// Tries to get the <see cref="IConfigElement"/> identified by <paramref name="key"/>, which must be able to be parsed as an int.
		/// If it does not exist, or <paramref name="key"/> parses to an int which is outside the range of <see cref="Elements"/>, throws a <see cref="KeyNotFoundException"/>.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		public IConfigElement GetElement(string key)
		{
			if (int.TryParse(key, out int index) && index >= 0 && Elements.Count > index)
			{
				return Elements[index];
			}
			else
			{
				throw new KeyNotFoundException(string.Concat("There is no ", nameof(ConfigStringElement), " with a key of ", key, " or the key cannot be parsed as an int as this is a ", nameof(ConfigArrayElement)));
			}
		}
		/// <summary>
		/// Tries to get the <see cref="IConfigElement"/> identified by <paramref name="key"/>, which must be able to be parsed as an int.
		/// If it does not exist, or <paramref name="key"/> parses to an int which is outside the range of <see cref="Elements"/>, returns <see cref="ConfigInvalidElement.Inst"/>.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		public IConfigElement TryGetElement(string key)
		{
			if (int.TryParse(key, out int index) && index >= 0 && Elements.Count > index)
			{
				return Elements[index];
			}
			else
			{
				return ConfigInvalidElement.Inst;
			}
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
	}
}

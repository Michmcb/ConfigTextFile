using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace ConfigTextFile
{
	/// <summary>
	/// Represents an array of strings within the ConfigFile.
	/// It has children but cannot have any Value set.
	/// </summary>
	public class ConfigArrayElement : IConfigElement
	{
		public ConfigArrayElement(string key, string path)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
		}
		public ConfigArrayElement(string key, string path, List<string> comments)
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
		public string this[string key]
		{
			get
			{
				return Elements.TryGetValue(key, out IConfigElement section) ? section.Value : null;
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
		public string Key { get; }
		public string Path { get; }
		/// <summary>
		/// Always returns an empty string. Setting this throws an InvalidOperationException.
		/// </summary>
		public string Value { get => ""; set => throw new InvalidOperationException("You cannot set the value of a ConfigArrayElement"); }
		/// <summary>
		/// Always true
		/// </summary>
		public bool IsValid => true;
		/// <summary>
		/// All IConfigElements within this Section.
		/// All of these are ConfigStringElements.
		/// </summary>
		public IDictionary<string, IConfigElement> Elements { get; }
		/// <summary>
		/// Returns ConfigElementType.Array
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Array;
		/// <summary>
		/// The comments that preceded this ConfigArrayElement
		/// </summary>
		public ICollection<string> Comments { get; set; }
		public IConfigElement GetElement(string key)
		{
			return Elements.TryGetValue(key, out IConfigElement section) ? section : ConfigInvalidElement.Inst;
		}
		public IConfigurationSection GetSection(string key)
		{
			return GetElement(key);
		}
		public IEnumerable<IConfigurationSection> GetChildren()
		{
			return Elements.Values;
		}
		/// <summary>
		/// A convenience method that loops over all strings in this array.
		/// </summary>
		public IEnumerable<string> GetValues()
		{
			foreach (IConfigElement e in Elements.Values)
			{
				yield return e.Value;
			}
		}
		public IChangeToken GetReloadToken()
		{
			throw new NotImplementedException("Currently you can't reload this, so Change Tokens are not implemented yet");
		}
		/// <summary>
		/// Returns Path
		/// </summary>
		public override string ToString()
		{
			return Path;
		}
		/// <summary>
		/// Never throws
		/// </summary>
		public void ThrowIfInvalid() { }
		public ConfigArrayElement AsArrayElement()
		{
			return this;
		}
		public ConfigSectionElement AsSectionElement()
		{
			throw new InvalidCastException("This is not a ConfigSectionElement; it is a ConfigArrayElement");
		}
		public ConfigStringElement AsStringElement()
		{
			throw new InvalidCastException("This is not a ConfigStringElement; it is a ConfigArrayElement");
		}
	}
}

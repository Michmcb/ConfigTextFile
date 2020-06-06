using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace ConfigTextFile
{
	/// <summary>
	/// Represents a section within the ConfigFile.
	/// It has children but cannot have any Value set.
	/// </summary>
	public class ConfigSectionElement : IConfigElement
	{
		public ConfigSectionElement(string key, string path)
		{
			Key = key;
			Path = path;
			Elements = new Dictionary<string, IConfigElement>();
		}
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
		public string Value { get => ""; set => throw new InvalidOperationException("You cannot set the value of a ConfigSectionElement"); }
		/// <summary>
		/// Always true
		/// </summary>
		public bool IsValid => true;
		/// <summary>
		/// All IConfigElements within this Section
		/// </summary>
		public IDictionary<string, IConfigElement> Elements { get; }
		/// <summary>
		/// Returns ConfigElementType.Section
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Section;
		/// <summary>
		/// The comments that preceded this ConfigSectionElement
		/// </summary>
		public ICollection<string> Comments { get; set; }
		public IEnumerable<IConfigurationSection> GetChildren()
		{
			return Elements.Values;
		}
		public IConfigurationSection GetSection(string key)
		{
			return GetElement(key);
		}
		public IConfigElement GetElement(string key)
		{
			return Elements.TryGetValue(key, out IConfigElement section) ? section : ConfigInvalidElement.Inst;
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
			throw new InvalidCastException("This is not a ConfigArrayElement; it is a ConfigSectionElement");
		}
		public ConfigSectionElement AsSectionElement()
		{
			return this;
		}
		public ConfigStringElement AsStringElement()
		{
			throw new InvalidCastException("This is not a ConfigStringElement; it is a ConfigSectionElement");
		}
	}
}

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
		/// <summary>
		/// Returns ConfigElementType.Array
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Array;
		public string Key { get; }
		public string Path { get; }
		/// <summary>
		/// Always returns an empty string. Setting this does nothing.
		/// </summary>
		public string Value { get => ""; set { } }
		/// <summary>
		/// All IConfigElements within this Section
		/// </summary>
		public IDictionary<string, IConfigElement> Elements { get; }
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
	}
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace ConfigTextFile
{
	/// <summary>
	/// Represents a single string within the ConfigFile.
	/// It does not have any children but it does have a Value.
	/// </summary>
	public class ConfigStringElement : IConfigElement
	{
		public ConfigStringElement(string key, string path, string value)
		{
			Key = key;
			Path = path;
			Value = value;
		}
		/// <summary>
		/// Always throws an InvalidOperationException, as strings don't have any children.
		/// </summary>
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
		public string Key { get; }
		public string Path { get; }
		public string Value { get; set; }
		/// <summary>
		/// Always throws an InvalidOperationException, as strings don't have any children.
		/// </summary>
		public IDictionary<string, IConfigElement> Elements => throw new InvalidOperationException("This is a ConfigStringElement; it has no children to get");
		/// <summary>
		/// Returns ConfigElementType.String
		/// </summary>
		public ConfigElementType Type => ConfigElementType.String;
		/// <summary>
		/// Returns an Empty sequence
		/// </summary>
		public IEnumerable<IConfigurationSection> GetChildren()
		{
			yield break;
		}
		/// <summary>
		/// Returns the ConfigInvalidElement instance
		/// </summary>
		public IConfigurationSection GetSection(string key)
		{
			return ConfigInvalidElement.Inst;
		}
		/// <summary>
		/// Returns the ConfigInvalidElement instance
		/// </summary>
		public IConfigElement GetElement(string key)
		{
			return ConfigInvalidElement.Inst;
		}
		public IChangeToken GetReloadToken()
		{
			throw new NotImplementedException("Currently you can't reload this, so Change Tokens are not implemented yet");
		}
		/// <summary>
		/// Returns Path=Value
		/// </summary>
		public override string ToString()
		{
			return string.Concat(Path, "=", Value ?? "(null)");
		}
		/// <summary>
		/// Never throws
		/// </summary>
		public void ThrowIfInvalid() { }
	}
}

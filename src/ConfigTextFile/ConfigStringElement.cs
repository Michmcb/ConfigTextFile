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
			Comments = new List<string>();
		}
		public ConfigStringElement(string key, string path, string value, IEnumerable<string> comments)
		{
			Key = key;
			Path = path;
			Value = value;
			Comments = new List<string>(comments);
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
		/// Always true
		/// </summary>
		public bool IsValid => true;
		/// <summary>
		/// Always throws an InvalidOperationException, as strings don't have any children.
		/// </summary>
		public IDictionary<string, IConfigElement> Elements => throw new InvalidOperationException("This is a ConfigStringElement; it has no children to get");
		/// <summary>
		/// Returns ConfigElementType.String
		/// </summary>
		public ConfigElementType Type => ConfigElementType.String;
		/// <summary>
		/// The comments that preceded this ConfigStringElement
		/// </summary>
		public ICollection<string> Comments { get; set; }
		/// <summary>
		/// Returns the ConfigInvalidElement instance
		/// </summary>
		public IConfigElement GetElement(string key)
		{
			return ConfigInvalidElement.Inst;
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
		public ConfigArrayElement AsArrayElement()
		{
			throw new InvalidCastException("This is not a ConfigArrayElement; it is a ConfigStringElement");
		}
		public ConfigSectionElement AsSectionElement()
		{
			throw new InvalidCastException("This is not a ConfigSectionElement; it is a ConfigStringElement");
		}
		public ConfigStringElement AsStringElement()
		{
			return this;
		}
		/// <summary>
		/// Returns an Empty sequence
		/// </summary>
		public IEnumerable<IConfigurationSection> GetChildren()
		{
			yield break;
		}
		public IChangeToken GetReloadToken()
		{
			throw new NotImplementedException("Currently you can't reload this, so Change Tokens are not implemented yet");
		}
		/// <summary>
		/// Returns the ConfigInvalidElement instance
		/// </summary>
		public IConfigurationSection GetSection(string key)
		{
			return ConfigInvalidElement.Inst;
		}
	}
}

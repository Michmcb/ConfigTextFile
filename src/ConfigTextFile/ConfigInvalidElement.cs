using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace ConfigTextFile
{
	/// <summary>
	/// Represents that a requested element did not exist.
	/// This is a singleton, and you can't do too much with this.
	/// Check individual methods and properties to see what they will always do; either empty strings, empty arrays, or throw a ConfigInvalidElementException.
	/// The singleton instance is ConfigInvalidElement.Inst, if you need it.
	/// You can still set the Value of this as per normal, so all invalid elements will have a specific string value, if that's useful to you.
	/// </summary>
	public class ConfigInvalidElement : IConfigElement
	{
		/// <summary>
		/// The singleton instance of ConfigInvalidElement
		/// </summary>
		public static readonly ConfigInvalidElement Inst = new ConfigInvalidElement("");
		private ConfigInvalidElement(string value)
		{
			Value = value;
		}
		/// <summary>
		/// Always throws an ConfigInvalidElementException.
		/// </summary>
		public string this[string key]
		{
			get
			{
				throw new ConfigInvalidElementException();
			}
			set
			{
				throw new ConfigInvalidElementException();
			}
		}
		/// <summary>
		/// Returns ConfigElementType.Invalid
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Invalid;
		/// <summary>
		/// Always throws an ConfigInvalidElementException.
		/// </summary>
		public IDictionary<string, IConfigElement> Elements => throw new ConfigInvalidElementException();
		/// <summary>
		/// Always throws a ConfigInvalidElementException
		/// </summary>
		public string Key => throw new ConfigInvalidElementException();
		/// <summary>
		/// Always throws a ConfigInvalidElementException
		/// </summary>
		public string Path => throw new ConfigInvalidElementException();
		/// <summary>
		/// By default, always an empty string. You can set this to whatever value you'd like to use to represent an Invalid value.
		/// </summary>
		public string Value { get; set; }
		/// <summary>
		/// Always false
		/// </summary>
		public bool IsValid => false;
		/// <summary>
		/// Always an empty sequence
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
		/// Always throws a ConfigInvalidElementException
		/// </summary>
		public IConfigurationSection GetSection(string key)
		{
			return Inst;
		}
		/// <summary>
		/// Always returns Inst
		/// </summary>
		public IConfigElement GetElement(string key)
		{
			return Inst;
		}
		/// <summary>
		/// Always throws
		/// </summary>
		public void ThrowIfInvalid()
		{
			throw new ConfigInvalidElementException();
		}
		public ConfigArrayElement AsArrayElement()
		{
			throw new ConfigInvalidElementException();
		}
		public ConfigSectionElement AsSectionElement()
		{
			throw new ConfigInvalidElementException();
		}
		public ConfigStringElement AsStringElement()
		{
			throw new ConfigInvalidElementException();
		}
	}
}

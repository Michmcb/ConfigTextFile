using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace ConfigTextFile
{
	/// <summary>
	/// Represents that a requested element did not exist.
	/// This is a singleton, and you can't really do anything with this.
	/// </summary>
	public class ConfigInvalidElement : IConfigElement
	{
		public const string ErrMsg = "This ConfigElement is Invalid. This usually means the key used to get this ConfigElement did not exist in the file that was loaded.";
		/// <summary>
		/// The singleton instance of ConfigInvalidElement
		/// </summary>
		public static readonly ConfigInvalidElement Inst = new ConfigInvalidElement();
		private ConfigInvalidElement() { }
		/// <summary>
		/// Always throws an InvalidOperationException.
		/// </summary>
		public string this[string key]
		{
			get
			{
				throw new InvalidOperationException(ErrMsg);
			}
			set
			{
				throw new InvalidOperationException(ErrMsg);
			}
		}
		/// <summary>
		/// Returns ConfigElementType.Invalid
		/// </summary>
		public ConfigElementType Type => ConfigElementType.Invalid;
		/// <summary>
		/// Always throws an InvalidOperationException.
		/// </summary>
		public IDictionary<string, IConfigElement> Elements => throw new InvalidOperationException(ErrMsg);
		/// <summary>
		/// Always an empty string
		/// </summary>
		public string Key => "";
		/// <summary>
		/// Always an empty string
		/// </summary>
		public string Path => "";
		/// <summary>
		/// Always an empty string, setter does nothing
		/// </summary>
		public string Value { get => ""; set { } }
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
		/// Always returns Inst
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
			// TODO Custom exception type
			throw new InvalidOperationException(ErrMsg);
		}
	}
}

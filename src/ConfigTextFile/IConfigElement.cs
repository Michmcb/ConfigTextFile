using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ConfigTextFile
{
	/// <summary>
	/// Represents a ConfigElement within a ConfigFile.
	/// </summary>
	public interface IConfigElement : IConfigurationSection
	{
		/// <summary>
		/// The Type of this element
		/// </summary>
		ConfigElementType Type { get; }
		/// <summary>
		/// Tries to get the ConfigElement identified by <paramref name="key"/>.
		/// If it does not exist, returns a <see cref="ConfigInvalidElement"/>.
		/// </summary>
		/// <param name="key">The key of the element</param>
		IConfigElement GetElement(string key);
		/// <summary>
		/// Child elements, if the Element is a Section or Array. In other cases, throws an InvalidOperationException.
		/// </summary>
		IDictionary<string, IConfigElement> Elements { get; }
		/// <summary>
		/// Throws if and only if this ConfigElement is a ConfigInvalidElement.
		/// </summary>
		void ThrowIfInvalid();
	}
}

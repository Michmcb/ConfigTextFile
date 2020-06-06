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
		/// Throws if and only if this IConfigElement is a ConfigInvalidElement.
		/// </summary>
		void ThrowIfInvalid();
		/// <summary>
		/// Returns false if this IConfigElement is a ConfigInvalidElement. Otherwise, returns true.
		/// </summary>
		bool IsValid { get; }
		/// <summary>
		/// Returns the IConfigElement as a ConfigArrayElement, if possible.
		/// Throws an InvalidCastException if this is not a ConfigArrayElement.
		/// (No cast actually takes place; simply returns this)
		/// </summary>
		ConfigArrayElement AsArrayElement();
		/// <summary>
		/// Returns the IConfigElement as a ConfigSectionElement, if possible.
		/// Throws an InvalidCastException if this is not a ConfigSectionElement
		/// (No cast actually takes place; simply returns this)
		/// </summary>
		ConfigSectionElement AsSectionElement();
		/// <summary>
		/// Returns the IConfigElement as a ConfigStringElement, if possible.
		/// Throws an InvalidCastException if this is not a ConfigStringElement
		/// (No cast actually takes place; simply returns this)
		/// </summary>
		ConfigStringElement AsStringElement();
	}
}

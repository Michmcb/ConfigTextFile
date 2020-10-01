namespace ConfigTextFile
{
	using System;
	using System.Collections.Generic;
	/// <summary>
	/// Represents a ConfigElement within a ConfigFile.
	/// </summary>
	public interface IConfigElement
	{
		/// <summary>
		/// Gets or sets a child element's value.
		/// <paramref name="key"/> should refer to a <see cref="ConfigStringElement"/>.
		/// If the key was not found, throws a <see cref="KeyNotFoundException"/>.
		/// If the key was found but it wasn't a <see cref="ConfigStringElement"/>, throws an <see cref="InvalidOperationException"/>.
		/// If this object is not a <see cref="ConfigSectionElement"/> or <see cref="ConfigArrayElement"/>, throws an <see cref="InvalidOperationException"/>.
		/// </summary>
		/// <param name="key">Gets or sets an <see cref="IConfigElement"/>'s Value whose Key property matches this.</param>
		string this[string key] { get; set; }
		/// <summary>
		/// The Key of this <see cref="IConfigElement"/>.
		/// </summary>
		string Key { get; }
		/// <summary>
		/// The full path to this <see cref="IConfigElement"/>.
		/// </summary>
		string Path { get; }
		/// <summary>
		/// Gets or sets this <see cref="IConfigElement"/>'s value.
		/// If this is not a <see cref="ConfigStringElement"/>, throws an <see cref="InvalidOperationException"/>.
		/// If this is a <see cref="ConfigInvalidElement"/>, throws a <see cref="ConfigInvalidElementException"/>.
		/// </summary>
		string Value { get; set; }
		/// <summary>
		/// The Type of this element.
		/// </summary>
		ConfigElementType Type { get; }
		/// <summary>
		/// Tries to get the <see cref="IConfigElement"/> identified by <paramref name="key"/>.
		/// If it does not exist, returns a <see cref="ConfigInvalidElement"/>.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		IConfigElement GetElement(string key);
		/// <summary>
		/// Throws if and only if this <see cref="IConfigElement"/> is a <see cref="ConfigInvalidElement"/>.
		/// </summary>
		void ThrowIfInvalid();
		/// <summary>
		/// Returns false if this <see cref="IConfigElement"/> is a <see cref="ConfigInvalidElement"/>. Otherwise, returns true.
		/// </summary>
		bool IsValid { get; }
		/// <summary>
		/// Returns the <see cref="IConfigElement"/> as a <see cref="ConfigArrayElement"/>, if possible.
		/// Throws an <see cref="InvalidOperationException"/> if this is not a <see cref="ConfigArrayElement"/>.
		/// (No cast actually takes place; simply returns this).
		/// </summary>
		ConfigArrayElement AsArrayElement();
		/// <summary>
		/// Returns the <see cref="IConfigElement"/> as a <see cref="ConfigSectionElement"/>, if possible.
		/// Throws an <see cref="InvalidOperationException"/> if this is not a <see cref="ConfigSectionElement"/>
		/// (No cast actually takes place; simply returns this).
		/// </summary>
		ConfigSectionElement AsSectionElement();
		/// <summary>
		/// Returns the <see cref="IConfigElement"/> as a <see cref="ConfigStringElement"/>, if possible.
		/// Throws an <see cref="InvalidOperationException"/> if this is not a <see cref="ConfigStringElement"/>
		/// (No cast actually takes place; simply returns this).
		/// </summary>
		ConfigStringElement AsStringElement();
	}
}

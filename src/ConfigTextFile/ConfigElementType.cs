namespace ConfigTextFile
{
	/// <summary>
	/// The type of an <see cref="IConfigElement"/>
	/// </summary>
	public enum ConfigElementType
	{
		/// <summary>
		/// A <see cref="ConfigInvalidElement"/>, typically used when an element failed to be found.
		/// </summary>
		Invalid,
		/// <summary>
		/// A <see cref="ConfigStringElement"/>, which is a key/value pair
		/// </summary>
		String,
		/// <summary>
		/// A <see cref="ConfigArrayElement"/>, which is a key with an array of values
		/// </summary>
		Array,
		/// <summary>
		/// A <see cref="ConfigSectionElement"/>, which is a key and holds strings/arrays
		/// </summary>
		Section
	}
}

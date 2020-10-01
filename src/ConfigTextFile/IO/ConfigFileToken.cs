namespace ConfigTextFile.IO
{
	using System;

	/// <summary>
	/// The type of a token
	/// </summary>
	[Flags]
	public enum ConfigFileToken
	{
		/// <summary>
		/// A key that comes before any of: <see cref="StartSection"/>, <see cref="StartArray"/>, <see cref="Value"/>.
		/// </summary>
		Key = 1,
		/// <summary>
		/// A single value that comes after <see cref="Key"/>. Does NOT include values within arrays!
		/// </summary>
		Value = 2,
		/// <summary>
		/// A comment
		/// </summary>
		Comment = 4,
		/// <summary>
		/// The start of an array; expect <see cref="ArrayValue"/> or <see cref="EndArray"/> to follow.
		/// </summary>
		StartArray = 8,
		/// <summary>
		/// A single value inside an array.
		/// </summary>
		ArrayValue = 16,
		/// <summary>
		/// The end of an array.
		/// </summary>
		EndArray = 32,
		/// <summary>
		/// The start of a section.
		/// </summary>
		StartSection = 64,
		/// <summary>
		/// The end of a section.
		/// </summary>
		EndSection = 128,
		/// <summary>
		/// The end of a config file.
		/// </summary>
		Finish = 256
	}
}

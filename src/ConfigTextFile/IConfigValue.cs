using System.Collections.Generic;

namespace ConfigTextFile
{
	/// <summary>
	/// A config value which can be either a single string or a collection of strings.
	/// </summary>
	public interface IConfigValue
	{
		/// <summary>
		/// True if this is a collection of strings, false if it is a single string.
		/// </summary>
		bool IsCollection { get; }
		/// <summary>
		/// True if this is a single string, false if it is a collection.
		/// </summary>
		bool IsSingular { get; }
		/// <summary>
		/// Gets the singular string.
		/// </summary>
		string Singular { get; }
		/// <summary>
		/// Gets the collection of strings.
		/// </summary>
		ICollection<string> Collection { get; }
	}
}

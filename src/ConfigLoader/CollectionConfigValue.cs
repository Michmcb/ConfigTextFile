using System;
using System.Collections.Generic;

namespace ConfigTextFile
{
	/// <summary>
	/// A config value which is a collection of strings
	/// </summary>
	public class CollectionConfigValue : IConfigValue
	{
		public CollectionConfigValue(ICollection<string> collection)
		{
			Collection = collection;
		}
		public CollectionConfigValue(params string[] collection)
		{
			Collection = collection;
		}
		public bool IsCollection => true;
		public bool IsSingular => false;
		public ICollection<string> Collection { get; set; }
		public string Singular => throw new InvalidOperationException("This is a collection, not a single value");
	}
}
using System;
using System.Collections.Generic;

namespace ConfigTextFile
{
	public class SingleConfigValue : IConfigValue
	{
		public SingleConfigValue(string singular)
		{
			Singular = singular;
		}
		public bool IsCollection => false;
		public bool IsSingular => true;
		public string Singular { get; set; }
		public ICollection<string> Collection => throw new InvalidOperationException("This is a single value, not a collection");
	}
}
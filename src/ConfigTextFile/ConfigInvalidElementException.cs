using System;
namespace ConfigTextFile
{
	public sealed class ConfigInvalidElementException : Exception
	{
		public ConfigInvalidElementException() : base("This ConfigElement is Invalid. This usually means the key used to get this ConfigElement did not exist in the file that was loaded.")
		{

		}
		public ConfigInvalidElementException(string message) : base(message)
		{
		}
		public ConfigInvalidElementException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

namespace ConfigTextFile
{
	using System;

	/// <summary>
	/// Indicates that a stream of text data could not be read as a config file.
	/// </summary>
	public sealed class ConfigFileFormatException : Exception
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public ConfigFileFormatException()
		{
		}
		public ConfigFileFormatException(string message) : base(message)
		{
		}
		public ConfigFileFormatException(string message, Exception innerException) : base(message, innerException)
		{
		}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}

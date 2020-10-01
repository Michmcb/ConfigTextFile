namespace ConfigTextFile
{
	using System;

	/// <summary>
	/// Thrown whenever you try to interact with a <see cref="ConfigInvalidElement"/>, such as trying to get/set its value.
	/// </summary>
	public sealed class ConfigInvalidElementException : Exception
	{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
		public ConfigInvalidElementException() : base("This " + nameof(IConfigElement) + " is Invalid. This usually means the key used to get this " + nameof(IConfigElement) + " did not exist in the " + nameof(ConfigFile) + " that was loaded.")
		{

		}
		public ConfigInvalidElementException(string message) : base(message)
		{
		}
		public ConfigInvalidElementException(string message, Exception innerException) : base(message, innerException)
		{
		}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
}

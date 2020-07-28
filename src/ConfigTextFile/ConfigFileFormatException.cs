﻿namespace ConfigTextFile
{
	using System;

	public sealed class ConfigFileFormatException : Exception
	{
		public ConfigFileFormatException()
		{
		}
		public ConfigFileFormatException(string message) : base(message)
		{
		}
		public ConfigFileFormatException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
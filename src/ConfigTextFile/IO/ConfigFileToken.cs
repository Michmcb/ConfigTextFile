namespace ConfigTextFile.IO
{
	using System;

	[Flags]
	public enum ConfigFileToken
	{
		Key = 1,
		Value = 2,
		Comment = 4,
		StartArray = 8,
		ArrayValue = 16,
		EndArray = 32,
		StartSection = 64,
		EndSection = 128,
		Finish = 256
	}
}

﻿namespace ConfigTextFile
{
	using System;

	/// <summary>
	/// Result of loading a <see cref="ConfigFile"/>
	/// </summary>
	public readonly struct LoadResult
	{
		/// <summary>
		/// Creates an instance with <see cref="ErrMsg"/> set to <see cref="string.Empty"/>
		/// </summary>
		/// <param name="configTextFile">The successfully loaded <see cref="ConfigFile"/>.</param>
		public LoadResult(ConfigFile configTextFile)
		{
			ConfigTextFile = configTextFile;
			ErrMsg = string.Empty;
		}
		/// <summary>
		/// Creates an instance with <see cref="ConfigTextFile"/> set to null.
		/// </summary>
		/// <param name="errMsg">The error message describing the failure.</param>
		public LoadResult(string errMsg)
		{
			ConfigTextFile = null;
			ErrMsg = errMsg;
		}
		/// <summary>
		/// The loaded text file. Null on failure.
		/// </summary>
		public ConfigFile? ConfigTextFile { get; }
		/// <summary>
		/// If <see cref="ConfigTextFile"/> is null, an error message. Otherwise, an empty string.
		/// </summary>
		public string ErrMsg { get; }
	}
}

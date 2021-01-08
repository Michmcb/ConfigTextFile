namespace ConfigTextFile.IO
{
	using System;

	/// <summary>
	/// An array of a config file
	/// </summary>
	public sealed class FluentConfigFileArray
	{
		private readonly ConfigFileWriter writer;
		internal FluentConfigFileArray(ConfigFileWriter writer)
		{
			this.writer = writer;
		}
		/// <summary>
		/// Writes a value into the array.
		/// </summary>
		/// <param name="value">The value to write</param>
		public void WriteValue(string value)
		{
			writer.WriteValue(value);
		}
#if !NETSTANDARD2_0
		/// <summary>
		/// Writes a value into the array.
		/// </summary>
		/// <param name="value">The value to write</param>
		public void WriteValue(in ReadOnlySpan<char> value)
		{
			writer.WriteValue(value);
		}
#endif
	}
}

namespace ConfigTextFile.IO
{
	using ConfigTextFile.IO.Fluent;
	using System;

	/// <summary>
	/// Provides a simpler way to write a syntactically valid config text file.
	/// The methods provided will also help prevent logic errors.
	/// </summary>
	public sealed class FluentConfigFileWriter : IDisposable
	{
		/// <summary>
		/// Creates a new instance which writes to <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The <see cref="ConfigFileWriter"/> to write to</param>
		/// <param name="closeOutput">If true, disposes of <paramref name="writer"/> when this object is disposed of</param>
		public FluentConfigFileWriter(ConfigFileWriter writer, bool closeOutput = true)
		{
			Writer = writer;
			CloseOutput = closeOutput;
		}
		/// <summary>
		/// The writer to use
		/// </summary>
		public ConfigFileWriter Writer { get; }
		/// <summary>
		/// If true, <see cref="Writer"/> will be closed when this is disposed. Otherwise, it will not.
		/// </summary>
		public bool CloseOutput { get; set; }
		/// <summary>
		/// Starts writing.
		/// </summary>
		/// <param name="root">The action to write the file</param>
		public void Write(Action<FluentConfigFileSection> root)
		{
			root(new FluentConfigFileSection(Writer, 0));
			Writer.Finished();
		}
		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
		/// <summary>
		/// Disposes of <see cref="Writer"/> if <see cref="CloseOutput"/> is true. Otherwise does nothing.
		/// </summary>
		public void Dispose()
		{
			if (!disposedValue)
			{
				if (CloseOutput)
				{
					Writer.Dispose();
				}

				disposedValue = true;
			}
		}
		#endregion
	}
}

namespace ConfigTextFile.IO
{
	using ConfigTextFile.IO.Fluent;
	using System;

	public sealed class FluentConfigFileWriter : IDisposable
	{
		/// <summary>
		/// Creates a new FluentConfigFileWriter which writes to <paramref name="writer"/>.
		/// Uses default formatting.
		/// </summary>
		/// <param name="writer">The StreamWriter to write to</param>
		/// <param name="closeOutput">If true, disposes of <paramref name="writer"/> when this object is disposed of</param>
		public FluentConfigFileWriter(ConfigFileWriter writer, bool closeOutput = true)
		{
			Writer = writer;
			Formatting = new ConfigFileFormatting();
			CloseOutput = closeOutput;
		}
		/// <summary>
		/// Creates a new FluentConfigFileWriter which writes to <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The ConfigFileWriter to write to</param>
		/// <param name="closeOutput">If true, disposes of <paramref name="writer"/> when this object is disposed of</param>
		public FluentConfigFileWriter(ConfigFileWriter writer, ConfigFileFormatting formatting, bool closeOutput = true)
		{
			Writer = writer;
			Formatting = formatting;
			CloseOutput = closeOutput;
		}
		public ConfigFileWriter Writer { get; }
		public ConfigFileFormatting Formatting { get; set; }
		public bool CloseOutput { get; set; }
		public void Write(Action<FluentConfigFileSection> root)
		{
			root(new FluentConfigFileSection(Writer, 0));
			Writer.Finished();
		}
		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
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

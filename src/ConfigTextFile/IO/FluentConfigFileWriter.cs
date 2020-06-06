using ConfigTextFile.IO.Fluent;
using System;

namespace ConfigTextFile.IO
{
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
		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (CloseOutput)
					{
						Writer.Dispose();
					}
				}

				disposedValue = true;
			}
		}
		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}
		#endregion
	}
}

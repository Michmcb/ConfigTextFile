namespace ConfigTextFile
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
	public readonly struct LoadResult
#pragma warning restore CA1815 // Override equals and operator equals on value types
	{
		/// <summary>
		/// Creates a new load result
		/// </summary>
		/// <param name="ctf">If success is false, this should be ConfigTextFile.Empty</param>
		public LoadResult(bool success, ConfigFile ctf, string errMsg)
		{
			Success = success;
			ConfigTextFile = ctf;
			ErrMsg = errMsg;
		}
		/// <summary>
		/// True if successful, false otherwise
		/// </summary>
		public bool Success { get; }
		/// <summary>
		/// The loaded text file. Never null.
		/// </summary>
		public ConfigFile ConfigTextFile { get; }
		/// <summary>
		/// If Success is false, an error message. Otherwise, an empty string.
		/// </summary>
		public string ErrMsg { get; }
	}
}

namespace ConfigTextFile.IO
{
	public enum ReadState
	{
		/// <summary>
		/// Indicates the next thing we expect to find is a Key, a Comment, the end of a section, or the end of the file
		/// </summary>
		Expecting_Key_Comment_EndSection_EndFile,
		/// <summary>
		/// Indicates we're reading elements of an array
		/// </summary>
		ReadingArray,
		/// <summary>
		/// Indicates we just read the start of a section
		/// </summary>
		AtStartOfSection,
		/// <summary>
		/// Indicates we just read the start of an array
		/// </summary>
		AtStartOfArray,
		/// <summary>
		/// Indicates we just read the end of an array
		/// </summary>
		AtEndOfArray,
		/// <summary>
		/// Indicates we just read either the start of an array or value, but we're not sure which (yet)
		/// </summary>
		AtStartOfArrayOrValue,
		/// <summary>
		/// Indicates we've reached the end of the file
		/// </summary>
		EndOfFile
	}
}

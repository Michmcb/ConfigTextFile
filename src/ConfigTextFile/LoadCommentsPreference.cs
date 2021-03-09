namespace ConfigTextFile
{
	/// <summary>
	/// Defines how to load comments.
	/// </summary>
	public enum LoadCommentsPreference
	{
		/// <summary>
		/// Loads comments normally.
		/// Use this when you need to re-save the file and thus preserve comments.
		/// </summary>
		Load,
		/// <summary>
		/// Ignores comments, and sets <see cref="IConfigElement.Comments"/> to null.
		/// Use this when you don't need to re-save the file.
		/// </summary>
		Ignore
	}
}
namespace ConfigTextFile.IO
{
	/// <summary>
	/// The order of preference for quoting strings
	/// </summary>
	public enum QuotePreference
	{
		/// <summary>
		/// " then ' then `
		/// </summary>
		Double_Single_Backtick,
		/// <summary>
		/// " then ` then '
		/// </summary>
		Double_Backtick_Single,
		/// <summary>
		/// ' then " then `
		/// </summary>
		Single_Double_Backtick,
		/// <summary>
		/// ' then ` then "
		/// </summary>
		Single_Backtick_Double,
		/// <summary>
		/// ` then " then '
		/// </summary>
		Backtick_Double_Single,
		/// <summary>
		/// ` then ' then "
		/// </summary>
		Backtick_Single_Double
	}
}

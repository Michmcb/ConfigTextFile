namespace ConfigTextFile.IO
{
	/// <summary>
	/// The order of preference for quoting strings
	/// </summary>
	public enum QuotePreference
	{
		Double_Single_Backtick,
		Double_Backtick_Single,
		Single_Double_Backtick,
		Single_Backtick_Double,
		Backtick_Double_Single,
		Backtick_Single_Double
	}
}

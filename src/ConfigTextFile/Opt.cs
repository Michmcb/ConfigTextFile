namespace ConfigTextFile
{
	internal readonly struct Opt<V> where V : class
	{
		internal Opt(V val, string errMsg = "")
		{
			Val = val;
			ErrMsg = errMsg;
		}
		internal V Val { get; }
		internal string ErrMsg { get; }
	}
}

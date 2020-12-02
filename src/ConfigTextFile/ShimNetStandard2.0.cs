#if NETSTANDARD2_0
#pragma warning disable IDE0060 // Remove unused parameter
namespace System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
	internal sealed class AllowNullAttribute : Attribute	{	}
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
	internal sealed class DisallowNullAttribute : Attribute {	}
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	internal sealed class NotNullWhenAttribute : Attribute {
		internal NotNullWhenAttribute(bool returnValue) { }
	}
}
#pragma warning restore IDE0060 // Remove unused parameter
internal static class Shims
{
	internal static bool Contains(this string s, char c)
	{
		return s.IndexOf(c) != -1;
	}
}
#endif
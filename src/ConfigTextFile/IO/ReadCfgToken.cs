using System;

namespace ConfigTextFile.IO
{
	/// <summary>
	/// Represents a single token that was read
	/// </summary>
	public readonly struct ReadCfgToken : IEquatable<ReadCfgToken>
	{
		public ReadCfgToken(string value, ConfigFileToken type)
		{
			Value = value;
			Type = type;
		}
		/// <summary>
		/// The value of the token that was read
		/// </summary>
		public string Value { get; }
		/// <summary>
		/// The type of token that was read. This is never more than one flag.
		/// </summary>
		public ConfigFileToken Type { get; }
		/// <summary>
		/// Returns true if Value and Type are the same
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is ReadCfgToken token && Equals(token);
		}
		/// <summary>
		/// Returns true if Value and Type are the same
		/// </summary>
		public bool Equals(ReadCfgToken other)
		{
			return Value == other.Value &&
				   Type == other.Type;
		}
		/// <summary>
		/// HashCode based on Value and Type
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			int hashCode = 1574892647;
			hashCode = hashCode * -1521134295 + Value.GetHashCode();
			hashCode = hashCode * -1521134295 + Type.GetHashCode();
			return hashCode;
		}
		/// <summary>
		/// Returns Value
		/// </summary>
		public override string ToString()
		{
			return Value;
		}
		public static bool operator ==(ReadCfgToken left, ReadCfgToken right)
		{
			return left.Equals(right);
		}
		public static bool operator !=(ReadCfgToken left, ReadCfgToken right)
		{
			return !(left == right);
		}
	}
}
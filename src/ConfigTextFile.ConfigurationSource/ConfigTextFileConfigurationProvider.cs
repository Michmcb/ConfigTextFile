namespace ConfigTextFile.ConfigurationSource
{
	using Microsoft.Extensions.Configuration;
	using System.IO;
	using System.Text;
	/// <summary>
	/// Loads a <see cref="ConfigFile"/>.
	/// </summary>
	public class ConfigTextFileConfigurationProvider : FileConfigurationProvider
	{
		public ConfigTextFileConfigurationProvider(ConfigTextFileConfigurationSource src) : base(src)
		{
			Encoding = src.Encoding;
		}
		public new ConfigFile Data { get; set; }
		/// <summary>
		/// The encoding to use.
		/// </summary>
		public Encoding Encoding { get; }
		/// <summary>
		/// Loads a <see cref="ConfigFile"/> from the provided <paramref name="stream"/>.
		/// Uses <see cref="Encoding"/> to load the file.
		/// </summary>
		/// <param name="stream">The stream to load from.</param>
		public override void Load(Stream stream)
		{
			ConfigFile cf = ConfigFile.LoadFile(new StreamReader(stream, Encoding), LoadCommentsPreference.Ignore);
			Data = cf;
			cf.FillStringDictionary(base.Data, overwrite: true);
		}
	}
}

namespace ConfigTextFile.ConfigurationSource
{
	using Microsoft.Extensions.Configuration;
	using System.Text;
	public class ConfigTextFileConfigurationSource : FileConfigurationSource
	{
		public override IConfigurationProvider Build(IConfigurationBuilder builder)
		{
			EnsureDefaults(builder);
			return new ConfigTextFileConfigurationProvider(this);
		}
		/// <summary>
		/// The Encoding that is passed to the <see cref="ConfigTextFileConfigurationProvider"/>.
		/// </summary>
		public Encoding Encoding { get; set; } = Encoding.UTF8;
	}
}

namespace ConfigTextFile.ConfigurationSource
{
	using Microsoft.Extensions.Configuration;
	using System.Text;
	/// <summary>
	/// A Configuration Source for a ConfigTextFile
	/// </summary>
	public class ConfigTextFileConfigurationSource : FileConfigurationSource
	{
		/// <summary>
		/// Builds the <see cref="IConfigurationProvider"/> for this source.
		/// </summary>
		/// <param name="builder">The <see cref="IConfigurationProvider"/>.</param>
		/// <returns>A <see cref="IConfigurationProvider"/>.</returns>
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

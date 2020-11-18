namespace ConfigTextFile.ConfigurationSource
{
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.FileProviders;
	using System;
	using System.Text;
	/// <summary>
	/// Extensions methods for adding a <see cref="ConfigTextFileConfigurationSource"/>.
	/// </summary>
	public static class ConfigTextFileConfigurationExtensions
	{
		/// <summary>
		/// Adds a ConfigTextFile configration provider for the file at <paramref name="path"/> to <paramref name="builder"/>.
		/// Uses <see cref="Encoding.UTF8"/> as the encoding. Is Required. Doesn't reload on change.
		/// </summary>
		/// <param name="builder">Adds it to this.</param>
		/// <param name="path">The path, relative to the base path <paramref name="builder"/> has.</param>
		/// <returns><paramref name="builder"/></returns>
		public static IConfigurationBuilder AddConfigTextFile(this IConfigurationBuilder builder, string path)
		{
			return AddConfigTextFile(builder, provider: null, path, Encoding.UTF8, optional: false, reloadOnChange: false);
		}
		/// <summary>
		/// Adds a ConfigTextFile configration provider for the file at <paramref name="path"/> to <paramref name="builder"/>.
		/// Is Required. Doesn't reload on change.
		/// </summary>
		/// <param name="builder">Adds it to this.</param>
		/// <param name="path">The path, relative to the base path <paramref name="builder"/> has.</param>
		/// <param name="encoding">The encoding to load the file as.</param>
		/// <returns><paramref name="builder"/></returns>
		public static IConfigurationBuilder AddConfigTextFile(this IConfigurationBuilder builder, string path, Encoding encoding)
		{
			return AddConfigTextFile(builder, provider: null, path, encoding, optional: false, reloadOnChange: false);
		}
		/// <summary>
		/// Adds a ConfigTextFile configration provider for the file at <paramref name="path"/> to <paramref name="builder"/>.
		/// Doesn't reload on change.
		/// </summary>
		/// <param name="builder">Adds it to this.</param>
		/// <param name="path">The path, relative to the base path <paramref name="builder"/> has.</param>
		/// <param name="encoding">The encoding to load the file as.</param>
		/// <param name="optional">Optional or required.</param>
		/// <returns><paramref name="builder"/></returns>
		public static IConfigurationBuilder AddConfigTextFile(this IConfigurationBuilder builder, string path, Encoding encoding, bool optional)
		{
			return AddConfigTextFile(builder, provider: null, path, encoding, optional, reloadOnChange: false);
		}
		/// <summary>
		/// Adds a ConfigTextFile configration provider for the file at <paramref name="path"/> to <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder">Adds it to this.</param>
		/// <param name="path">The path, relative to the base path <paramref name="builder"/> has.</param>
		/// <param name="encoding">The encoding to load the file as.</param>
		/// <param name="optional">Optional or required.</param>
		/// <param name="reloadOnChange">Should configuration reload if the file changes?</param>
		/// <returns><paramref name="builder"/></returns>
		public static IConfigurationBuilder AddConfigTextFile(this IConfigurationBuilder builder, string path, Encoding encoding, bool optional, bool reloadOnChange)
		{
			return AddConfigTextFile(builder, provider: null, path, encoding, optional, reloadOnChange);
		}
		/// <summary>
		/// Adds a ConfigTextFile configration provider for the file at <paramref name="path"/> to <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder">Adds it to this.</param>
		/// <param name="provider">File provider. If null, a PhysicalFileProvider is created for the nearest existing directory.</param>
		/// <param name="path">The path, relative to the base path <paramref name="builder"/> has.</param>
		/// <param name="encoding">The encoding to load the file as.</param>
		/// <param name="optional">Optional or required.</param>
		/// <param name="reloadOnChange">Should configuration reload if the file changes?</param>
		/// <returns><paramref name="builder"/></returns>
		public static IConfigurationBuilder AddConfigTextFile(this IConfigurationBuilder builder, IFileProvider? provider, string path, Encoding encoding, bool optional, bool reloadOnChange)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder), "builder cannot be null");
			}
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("path cannot be null or empty", nameof(path));
			}
			ConfigTextFileConfigurationSource src = new ConfigTextFileConfigurationSource()
			{
				FileProvider = provider,
				Path = path,
				Optional = optional,
				ReloadOnChange = reloadOnChange,
				Encoding = encoding
			};
			src.ResolveFileProvider();
			return builder.Add(src);
		}
	}
}

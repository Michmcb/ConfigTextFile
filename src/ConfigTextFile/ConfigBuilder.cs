namespace ConfigTextFile
{
	using System;
	using System.Collections.Generic;
	/// <summary>
	/// Allows you to build config elements in a fluent way.
	/// </summary>
	public readonly struct ConfigBuilder
	{
		private readonly ConfigSectionElement section;
		/// <summary>
		/// Creates a new instance which adds elements to <paramref name="section"/>.
		/// </summary>
		/// <param name="section">The section to add elements to.</param>
		public ConfigBuilder(ConfigSectionElement section)
		{
			this.section = section;
		}
		/// <summary>
		/// Creates a new <see cref="ConfigBuilder"/> and passes the instance to <paramref name="build"/>.
		/// </summary>
		/// <param name="root">The section to add elements to.</param>
		/// <param name="build">An action used to build <paramref name="root"/>.</param>
		/// <returns><paramref name="root"/>.</returns>
		public static ConfigSectionElement Build(ConfigSectionElement root, Action<ConfigBuilder> build)
		{
			build(new ConfigBuilder(root));
			return root;
		}
		/// <summary>
		/// Adds a new <see cref="ConfigStringElement"/> to the current section.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="value">The value.</param>
		/// <returns>This object allowing for chained calls.</returns>
		public ConfigBuilder String(string key, string value)
		{
			section.AddElement(new ConfigStringElement(key, value));
			return this;
		}
		/// <summary>
		/// Adds a new <see cref="ConfigStringElement"/> to the current section.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="value">The value.</param>
		/// <param name="comments">The comments to use. If <paramref name="copyComments"/> is true they are copied, otherwise they are used directly.</param>
		/// <param name="copyComments">If true, copies <paramref name="comments"/> into a new list. Otherwise, assigns directly.</param>
		/// <returns>This object allowing for chained calls.</returns>
		public ConfigBuilder String(string key, string value, ICollection<string> comments, bool copyComments = true)
		{
			section.AddElement(new ConfigStringElement(key, value, comments, copyComments));
			return this;
		}
		/// <summary>
		/// Adds a new <see cref="ConfigArrayElement"/> to the current section.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="values">The values; creates a new <see cref="ConfigStringElement"/> for every string in this collection.</param>
		/// <returns>This object allowing for chained calls.</returns>
		public ConfigBuilder Array(string key, IEnumerable<string> values)
		{
			section.AddElement(new ConfigArrayElement(key, values));
			return this;
		}
		/// <summary>
		/// Adds a new <see cref="ConfigArrayElement"/> to the current section.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="values">The values; creates a new <see cref="ConfigStringElement"/> for every string in this collection.</param>
		/// <param name="comments">The comments to use. If <paramref name="copyComments"/> is true they are copied, otherwise they are used directly.</param>
		/// <param name="copyComments">If true, copies <paramref name="comments"/> into a new list. Otherwise, assigns directly.</param>
		/// <returns>This object allowing for chained calls.</returns>
		public ConfigBuilder Array(string key, IEnumerable<string> values, ICollection<string> comments, bool copyComments = true)
		{
			section.AddElement(new ConfigArrayElement(key, values, comments, copyComments));
			return this;
		}
		/// <summary>
		/// Adds a new <see cref="ConfigArrayElement"/> to the current section.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="values">The values; creates a new <see cref="ConfigStringElement"/> for every string in this collection.</param>
		/// <returns>This object allowing for chained calls.</returns>
		public ConfigBuilder Array(string key, params string[] values)
		{
			section.AddElement(new ConfigArrayElement(key, values));
			return this;
		}
		/// <summary>
		/// Adds a new <see cref="ConfigSectionElement"/> to the current section.
		/// Uses the same <see cref="IEqualityComparer{T}"/> as the parent section.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="build">An action used to build the newly added section.</param>
		/// <returns>This object allowing for chained calls.</returns>	
		public ConfigBuilder Section(string key, Action<ConfigBuilder> build)
		{
			ConfigSectionElement subSection = new ConfigSectionElement(key, section.KeyComparer);
			section.AddElement(subSection);
			build(new ConfigBuilder(subSection));
			return this;
		}
		/// <summary>
		/// Adds a new <see cref="ConfigSectionElement"/> to the current section.
		/// Uses the same <see cref="IEqualityComparer{T}"/> as the parent section.
		/// </summary>
		/// <param name="key">This element's key</param>
		/// <param name="build">An action used to build the newly added section.</param>
		/// <param name="comments">The comments to use. If <paramref name="copyComments"/> is true they are copied, otherwise they are used directly.</param>
		/// <param name="copyComments">If true, copies <paramref name="comments"/> into a new list. Otherwise, assigns directly.</param>
		/// <returns>This object allowing for chained calls.</returns>	
		public ConfigBuilder Section(string key, ICollection<string> comments, bool copyComments, Action<ConfigBuilder> build)
		{
			ConfigSectionElement subSection = new ConfigSectionElement(key, section.KeyComparer, comments, copyComments);
			section.AddElement(subSection);
			build(new ConfigBuilder(subSection));
			return this;
		}
	}
}

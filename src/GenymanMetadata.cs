using System.Linq;
using System.Reflection;

namespace Genyman.Core
{
	public class GenymanMetadata
	{
		public GenymanMetadata()
		{
			var calling = Assembly.GetEntryAssembly();
			var assemblyName = calling.GetName();
			Version = ""; // empty defaults to latest version
			PackageId = assemblyName.Name;
			NugetSource = ""; // empty defaults to standard nuget source

			Description = calling.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
		}

		public string PackageId { get; set; }
		public string Version { get; set; }
		public string NugetSource { get; set; }
		public string Info { get; } = "This is a Genyman configuration file - https://genyman.github.io/docs/";

		internal string Identifier => PackageId.Split(".").Last();
		internal string Description { get; set; }
	}
}
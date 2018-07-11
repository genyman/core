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
			Version = $"{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}";
			PackageId = assemblyName.Name;
			NugetSource = "";

			Description = calling.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
		}

		public string PackageId { get; set; }
		public string Version { get; set; }
		public string NugetSource { get; set; }

		internal string Identifier => PackageId.Split(".").Last();
		internal string Description { get; set; }
	}
}
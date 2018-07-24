using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Genyman.Core.Helpers
{
	public static class CSharpProjectHelpers
	{
		internal const string DotNetXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

		public static void AddXamarinIosResource(this string csprojFolder, string resourceFileName)
		{
			AddXamarinResource(csprojFolder, resourceFileName, "BundleResource");
		}
		
		public static void AddXamarinAndroidResource(this string csprojFolder, string resourceFileName)
		{
			AddXamarinResource(csprojFolder, resourceFileName, "AndroidResource");
		}
		
		public static void AddXamarinUWPResource(this string csprojFolder, string resourceFileName)
		{
			AddXamarinResource(csprojFolder, resourceFileName, "Content");
		}

		internal static void AddXamarinResource(this string csprojFolder, string resourceFileName, string node)
		{
			var csProj = Directory.GetFiles(csprojFolder, "*.csproj").First();
			resourceFileName = GetRelativeFileName(csProj, resourceFileName).Replace("/", "\\");
			var bundleResource = $"<{node} Include=\"{resourceFileName}\" />";
			var contents = File.ReadAllLines(csProj);
			if (contents.Any(s => s.Trim() == bundleResource)) return;
			var index = Array.FindLastIndex(contents, s => s.Trim().StartsWith($"<{node}"));

			var target = new List<string>(contents);
			if (index < 0)
			{
				// we need to add a new group
				//</ItemGroup>
				var lastIndex = Array.FindLastIndex(contents, s => s.Trim().StartsWith("</ItemGroup>"));
				target.Insert(lastIndex + 1, "  <ItemGroup>");
				target.Insert(lastIndex + 2, $"    {bundleResource}");
				target.Insert(lastIndex + 3, "  </ItemGroup>");
			}
			else
			{
				target.Insert(index + 1, $"    {bundleResource}");
			}

			File.WriteAllLines(csProj, target.ToArray());
		}

		static string GetRelativeFileName(string baseFileName, string fileName)
		{
			var baseFolder = new FileInfo(baseFileName).DirectoryName;
			var result = fileName.Replace(baseFolder, "").Substring(1);
			return result;
		}
	}
}
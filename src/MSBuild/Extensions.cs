using System;
using System.IO;

namespace Genyman.Core.MSBuild
{
	internal static class Extensions
	{
		public static string ToPlatformPath(this string fileName)
		{
			// we need to replace all slashes and forward slashes to what the current environment is using
			return fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
		}

		public static string ToMSBuildPath(this string fileName)
		{
			// in MSBuild the \ is always used for path separator
			return fileName.Replace('/', '\\');
		}

		public static string GetParentSolution(this string fileName)
		{
			var result = GetParentFile(fileName, "sln");
			if (result == null) throw new FileNotFoundException($"No Solution found for {fileName}");
			return result;
		}

		public static string GetParentProject(this string fileName)
		{
			var result = GetParentFile(fileName, "csproj", "projitems");
			if (result == null) throw new FileNotFoundException($"No Project found for {fileName}");
			return result;
		}

		public static string GetParentSharedProject(this string fileName)
		{
			var result = GetParentFile(fileName, "projitems", "csproj");
			if (result == null) throw new FileNotFoundException($"No Shared Project found for {fileName}");
			return result;
		}

		static string GetParentFile(string fileName, string parentExtension, string breakExtension = null)
		{
			if (!File.Exists(fileName))
				throw new FileNotFoundException();

			// if not set, we just set it to something that doesn't exists
			if (breakExtension == null) breakExtension = "BREAKEXTENSIONS";
			var fileInfo = new FileInfo(fileName);
			var exit = false;
			var directoryInfo = fileInfo.Directory;
			while (!exit)
			{
				var resultParent = Find(directoryInfo, parentExtension);
				var resultBreak = Find(directoryInfo, breakExtension);

				// if we found already an item with the break extension, early exit
				if (resultBreak.found) return null;
				if (resultParent.found) return resultParent.file;
				directoryInfo = directoryInfo.Parent;
				if (directoryInfo == null) exit = true;
			}

			return null;

			(bool found, string file) Find(DirectoryInfo directory, string extension)
			{
				if (directory == null) throw new ArgumentNullException(nameof(directory));
				var foundParentFile = Directory.GetFiles(directory.FullName, $"*.{extension}");
				return foundParentFile.Length != 0 ? (true, foundParentFile[0]) : (false, null);
			}
		}
	}
}
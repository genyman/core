using System;
using System.IO;

namespace Genyman.Core.Helpers
{
	public static class FileHelpers
	{
		public static string EnsureFolderExists(this string fileName)
		{
			string path = null;
			try
			{
				var fi = new FileInfo(fileName);
				path = fi.DirectoryName;

				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				return path;
			}
			catch (Exception e)
			{
				Log.Fatal($"Could not create folder {path}");
			}

			return null;
		}
	}
}
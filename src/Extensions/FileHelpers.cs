using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Genyman.Core
{
	public static class FileExtensions
	{
		public static string EnsureFolderExists(this string fileName)
		{
			string path = null;
			try
			{
				path = Path.GetDirectoryName(fileName);
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				return path;
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				Log.Fatal($"Could not create folder {path}");
			}

			return null;
		}
	}
}
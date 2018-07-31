using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Genyman.Core.MSBuild
{
	public class Solution
	{
		// https://sourceforge.net/projects/syncproj/

		private string _fileName;

		internal Solution()
		{
		}

		public string Name { get; set; }

		public string FileName
		{
			get => _fileName;
			internal set => _fileName = value.ToPlatformPath();
		}

		public bool Loaded { get; internal set; }
		public Exception Exception { get; internal set; }
		public List<Project> Projects { get; internal set; } = new List<Project>();
		public List<SharedProject> SharedProjects { get; internal set; } = new List<SharedProject>();

		public static Solution Load(string solutionFileName)
		{
			var solution = new Solution {FileName = solutionFileName};
			try
			{
				if (!File.Exists(solutionFileName))
					throw new FileNotFoundException();

				var fileInfo = new FileInfo(solutionFileName);
				solution.Name = fileInfo.Name.Replace(fileInfo.Extension, "");
				solution.FileName = fileInfo.FullName;

				var contents = File.ReadAllLines(solutionFileName);
				if (!contents.Contains("Project") && !contents.Contains("EndProject"))
					throw new Exception("Not a valid solution file");

				string line;
				var lineIndex = 0;
				do
				{
					line = contents[lineIndex];
					if (line.StartsWith("Project"))
					{
						var parts = line.Split('=', ',');
						var projectFileName = Path.Combine(fileInfo.DirectoryName, parts[2].Replace("\"", "").Trim()).ToPlatformPath();
						if (projectFileName.EndsWith("csproj", StringComparison.OrdinalIgnoreCase))
						{
							solution.Projects.Add(new Project() {FileName = projectFileName});
						}
						else if (projectFileName.EndsWith("shproj", StringComparison.OrdinalIgnoreCase))
						{
							solution.SharedProjects.Add(new SharedProject() {FileName = projectFileName});
						}
					}
					lineIndex++;
				} while (line != "EndProject");
				solution.Loaded = true;
			}
			catch (Exception e)
			{
				solution.Exception = e;
			}
			return solution;
		}

		internal static Solution LoadFromFile(string fileName)
		{
			var solution = new Solution();
			try
			{
				var solutionFileName = fileName.GetParentSolution();
				return Load(solutionFileName);
			}
			catch (Exception e)
			{
				solution.Exception = e;
			}
			return solution;
		}
	}
}
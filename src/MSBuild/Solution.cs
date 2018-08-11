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
		protected List<string> Contents { get; set; }


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

				solution.Contents = File.ReadAllLines(solutionFileName).ToList();
				if (!solution.Contents.Contains("Project") && !solution.Contents.Contains("EndProject"))
					throw new Exception("Not a valid solution file");

				string line;
				var lineIndex = 0;
				do
				{
					line = solution.Contents[lineIndex];
					if (line.StartsWith("Project"))
					{
						var parts = line.Split('=', ',');
						var projectFileName = Path.Combine(fileInfo.DirectoryName, parts[2].Replace("\"", "").Trim()).ToPlatformPath();
						if (projectFileName.EndsWith("csproj", StringComparison.OrdinalIgnoreCase))
						{
							var project = Project.Load(projectFileName, parts[1].Replace("\"", "").Trim());
							if (project.NotFound) project.Id = parts.Last().Replace("\"", "").Trim(); // if not found, still add the ID here
							solution.Projects.Add(project);
						}
						else if (projectFileName.EndsWith("shproj", StringComparison.OrdinalIgnoreCase))
						{
							var project = SharedProject.Load(projectFileName, parts[1].Replace("\"", "").Trim());
							if (project.NotFound) project.Id = parts.Last().Replace("\"", "").Trim(); // if not found, still add the ID here
							solution.SharedProjects.Add(project);
						}
					}

					lineIndex++;
				} while (line != "Global");

				solution.Loaded = true;
			}
			catch (Exception e)
			{
				solution.Exception = e;
			}

			return solution;
		}

		public void Remove(Project project)
		{
			Log.Debug($"Removing {project.Id} {project.FileName} from Solution");

			var startProjectIndex = Contents.FindIndex(q => q.EndsWith($"{project.Id}\""));
			Contents.RemoveAt(startProjectIndex);
			Contents.RemoveAt(startProjectIndex); // = EndProject

			var startProjectConfigIndex = Contents.FindIndex(q => q.Trim().StartsWith(project.Id));
			if (startProjectConfigIndex != -1)
			{
				do
				{
					Contents.RemoveAt(startProjectConfigIndex);
				} while (Contents[startProjectConfigIndex].Trim().StartsWith(project.Id));
			}

			File.WriteAllLines(FileName, Contents);
		}

		public void Remove(ProjectSubType projectSubType)
		{
			var projects = Projects.Where(q => q.SubType == projectSubType);
			foreach (var project in projects)
			{
				Remove(project);
			}
		}

		public void RemoveConfigurations(ProjectSubType projectSubType)
		{
			// at the moment we just clean up iOS configurations
			if (projectSubType != ProjectSubType.XamarinIOS)
			{
				Log.Debug($"No configuration clean up in this solution supported for {projectSubType}");
				return;
			}

			// iOS
			var toRemove = new List<string>() {"Debug|iPhoneSimulator", "Release|iPhoneSimulator", "Debug|iPhone", "Release|iPhone", "Ad-Hoc|iPhone", "AppStore|iPhone"};
			foreach (var remove in toRemove)
				Contents.RemoveAll(s => s.Contains(remove));
			File.WriteAllLines(FileName, Contents);
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
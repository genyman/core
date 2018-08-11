using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Genyman.Core.MSBuild
{
	public class SharedProject : Project
	{
		internal SharedProject()
		{
			IsSharedProject = true;
		}

		internal string ShProjFileName { get; set; }

		protected override void Load()
		{
			if (Loaded) return;

			try
			{
				if (!File.Exists(FileName))
					throw new FileNotFoundException();

				var fileInfo = new FileInfo(FileName);
				Name = fileInfo.Name.Replace(fileInfo.Extension, "");
				FileName = fileInfo.FullName;
				ProjectDirectory = fileInfo.DirectoryName;

				ShProjFileName = $"{Name}.shproj";

				_contents = System.IO.File.ReadAllLines(FileName).ToList();
				RootNamespace = FindProperty("Import_RootNamespace");

				Loaded = true;
			}
			catch (Exception e)
			{
				Exception = e;
			}
		}

		internal new static SharedProject Load(string projectFileName, string solutionIdentifier = null)
		{
			var sharedProject = new SharedProject {FileName = projectFileName};
			sharedProject.Load();
			sharedProject.SolutionIdentifier = solutionIdentifier;
			return sharedProject;
		}

		internal new static SharedProject LoadFromFile(string fileName)
		{
			var sharedProject = new SharedProject();
			try
			{
				var projectFileName = fileName.GetParentSharedProject();
				return Load(projectFileName);
			}
			catch (Exception e)
			{
				sharedProject.Exception = e;
			}

			return sharedProject;
		}
	}
}
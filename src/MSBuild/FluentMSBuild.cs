using System.IO;

namespace Genyman.Core.MSBuild
{
	public class FluentMSBuild
	{
		string FileName { get; set; }
		BuildAction BuildAction { get; set; }
		string DependencyOn { get; set; }
		string CustomTool { get; set; }

		Solution _solution;
		Project _project;
		SharedProject _sharedProject;

		public Solution Solution
		{
			get => _solution ?? (_solution = Solution.LoadFromFile(FileName));
			private set => _solution = value;
		}
				
		public Project Project
		{
			get => _project ?? (_project = Project.LoadFromFile(FileName));
			private set => _project = value;
		}
		
		public SharedProject SharedProject
		{
			get => _sharedProject ?? (_sharedProject = SharedProject.LoadFromFile(FileName));
			private set => _sharedProject = value;
		}
		
		FluentMSBuild()
		{
		}

		public static FluentMSBuild Use(string fileName)
		{
			if (!File.Exists(fileName))
				throw new FileNotFoundException(fileName);

			return new FluentMSBuild
			{
				FileName = fileName
			};
		}


		public FluentMSBuild WithBuildAction(BuildAction buildAction)
		{
			BuildAction = buildAction;
			return this;
		}

		public FluentMSBuild WithDependencyOn(string fileName)
		{
			if (!File.Exists(fileName))
				throw new FileNotFoundException(fileName);

			DependencyOn = fileName;
			return this;
		}

		public FluentMSBuild WithCustomTool(string customTool)
		{
			CustomTool = customTool;
			return this;
		}

		public FluentMSBuild AddToProject()
		{
			var fileInfo = new FileInfo(FileName);
			AddFile(fileInfo.FullName, BuildAction, DependencyOn, CustomTool);
			return this;
		}

		public FluentMSBuild IncrementVersion(bool major, bool minor, bool build)
		{
			Project.IncrementVersion(major, minor, build);
			return this;
		}

		bool AddFile(string target, BuildAction buildAction, string dependentUpon = null, string customTool = null)
		{
			if (!Project.Loaded && !SharedProject.Loaded) return false;
			if (Project.Loaded)
				return Project.AddFile(target, buildAction, dependentUpon, customTool);
			if (SharedProject.Loaded)
				return SharedProject.AddFile(target, buildAction, dependentUpon, customTool);
			return false;
		}
	}
}
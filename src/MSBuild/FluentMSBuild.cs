using System.IO;

namespace Genyman.Core.MSBuild
{
	public class FluentMSBuild
	{
		string FileName { get; set; }
		BuildAction BuildAction { get; set; }
		string DependencyOn { get; set; }
		string CustomTool { get; set; }

		public Solution Solution { get; private set; }
		public Project Project { get; private set; }
		public SharedProject SharedProject { get; private set; }

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

			Solution = Solution.LoadFromFile(FileName);
			Project = Project.LoadFromFile(FileName);
			SharedProject = SharedProject.LoadFromFile(FileName);

			AddFile(fileInfo.FullName, BuildAction, DependencyOn, CustomTool);
			return this;
		}

		public FluentMSBuild IncrementVersion(bool major, bool minor, bool build)
		{
			Project = Project.Load(FileName);
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
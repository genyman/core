using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Genyman.Core;
using Genyman.Core.MSBuild;

namespace Sample.Genyman.MSBuildSolution.Implementation
{
	internal class Generator : GenymanGenerator<Configuration>
	{
		public override void Execute()
		{
			ProcessHandlebarTemplates(SkipTemplate, null, TemplateProcessed);
		}

		void TemplateProcessed(TemplateProcessed obj)
		{
			if (obj.Template.EndsWith("XamarinApp.sln"))
			{
				var solution = FluentMSBuild.Use(obj.File).Solution;
				if (!Configuration.UseIOS)
				{
					var projects = solution.Projects.Where(q => q.SubType == ProjectSubType.XamarinIOS || q.NotFound);
					RemoveProjects(projects, solution);
				}

				if (!Configuration.UseAndroid)
				{
					var projects = solution.Projects.Where(q => q.SubType == ProjectSubType.XamarinAndroid || q.NotFound);
					RemoveProjects(projects, solution);
				}
				
				// note: Because we are Skipping all files, including the csproj file, the above code can be simplified; you only have to check on NotFound property
			}

			void RemoveProjects(IEnumerable<Project> projects, Solution solution)
			{
				foreach (var project in projects)
				{
					solution.Remove(project);
				}
			}
		}

		bool SkipTemplate(string template)
		{
			if (!Configuration.UseIOS && template.Contains("XamarinApp.iOS")) return true;
			if (!Configuration.UseAndroid && template.Contains("XamarinApp.Android")) return true;
			return false;
		}
	}
}
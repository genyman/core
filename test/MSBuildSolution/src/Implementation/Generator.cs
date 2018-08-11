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
				// we have to check based upon the SolutionIdentifier because when this file is processed the projects itself are not copied yet (or are skipped)
				// therefore they will be marked as NotFound, and not able to identity the type; solutionIdentifier can always be used
				
				var solution = FluentMSBuild.Use(obj.File).Solution;
				if (!Configuration.UseIOS)
				{
					var projects = solution.Projects.Where(q => q.SolutionIdentifier == "XamarinApp.iOS");
					RemoveProjects(projects, solution);
					solution.RemoveConfigurations(ProjectSubType.XamarinIOS);
				}

				if (!Configuration.UseAndroid)
				{
					var projects = solution.Projects.Where(q => q.SolutionIdentifier == "XamarinApp.Android");
					RemoveProjects(projects, solution);
				}
				
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
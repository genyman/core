using Genyman.Core;
using Genyman.Core.MSBuild;

namespace Sample.Genyman.ConditionalTemplates.Implementation
{
	internal class Generator : GenymanGenerator<Configuration>
	{
		public override void Execute()
		{
			ProcessHandlebarTemplates(SkipTemplate, OverrideTargetName, TemplateProcessed);
		}

		void TemplateProcessed(TemplateProcessed templateProcessed)
		{
			Log.Debug($"TemplateProcessed");
			Log.Debug($"{templateProcessed.Template}");
			Log.Debug($"{templateProcessed.File}");
			// You can do something with this file. For example:
			// FluentMSBuild.Use(templateProcessed.File).AddToProject();
		}

		string OverrideTargetName(string arg)
		{
			Log.Debug($"OverrideTargetName - {arg}");
			if (arg == "Folder1/MyClass2.cs") return "Folder1/MyClass2-NewFile.cs";
			return arg;
		}

		bool SkipTemplate(string arg)
		{
			Log.Debug($"SkipTemplate - {arg}");
			if (arg == "Folder1/MyClass3.cs") return true;
			return false;
		}
	}
}
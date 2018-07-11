using System;
using System.IO;
using System.Reflection;
using Genyman.Core.Handlebars;
using Genyman.Core.Helpers;

namespace Genyman.Core
{
	public abstract class GenymanGenerator<T> where T : class
	{
		public T Configuration { get; internal set; }

		public GenymanMetadata ConfigurationMetadata { get; internal set; }

		public GenymanMetadata Metadata { get; internal set; }

		public string InputFileName { get; internal set; }

		public string WorkingDirectory { get; internal set; }
		
		public bool Overwrite { get; internal set; }

		protected string TemplatePath { get; }

		protected GenymanGenerator()
		{
			TemplatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Templates");
		}

		public abstract void Execute();

		public string TargetFileName(string templateFileName, string templatePath)
		{
			var result = templateFileName.Replace(templatePath, "").Substring(1);
			return result;
		}

		protected void ProcessHandlebarTemplates(Func<string, string> overrideTargetName = null)
		{
			var folder = TemplatePath;
			var templateFiles = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
			foreach (var templateFile in templateFiles)
			{
				var template = TargetFileName(templateFile, TemplatePath);
				var targetName = template;

				if (overrideTargetName != null)
				{
					var result = overrideTargetName.Invoke(template);
					if (!string.IsNullOrEmpty(result))
						targetName = result;
				}

				var targetPath = Path.Combine(WorkingDirectory, targetName);
				Log.Debug($"Processing {template}. Target: {targetPath}");

				targetPath.EnsureFolderExists();

				FluentHandlebars.Create(this)
					.HavingModel(Configuration)
					.UsingFileTemplate(templateFile)
					.OutputFile(targetPath, Overwrite);
			}
		}
	}
}
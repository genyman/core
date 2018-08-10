using System;
using System.IO;
using System.Reflection;
using Genyman.Core.Handlebars;

namespace Genyman.Core
{
	public abstract class GenymanGenerator<T> where T : class
	{
		protected GenymanGenerator()
		{
			TemplatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Templates");
		}

		public T Configuration { get; internal set; }

		public GenymanMetadata ConfigurationMetadata { get; internal set; }

		public GenymanMetadata Metadata { get; internal set; }

		public string InputFileName { get; internal set; }

		public string WorkingDirectory { get; internal set; }

		public bool Overwrite { get; set; }

		internal bool Update { get; set; }

		protected string TemplatePath { get; }

		public abstract void Execute();

		public string TargetFileName(string templateFileName, string templatePath)
		{
			var result = templateFileName.Replace(templatePath, "").Substring(1);
			return result;
		}

		protected void ProcessHandlebarTemplates(Func<string, bool> skipTemplate = null, Func<string, string> overrideTargetName = null,
			Action<TemplateProcessed> templateProcessed = null)
		{
			var folder = TemplatePath;
			var templateFiles = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
			foreach (var templateFile in templateFiles)
			{
				var template = TargetFileName(templateFile, TemplatePath);
				var targetName = template;

				if (skipTemplate != null)
				{
					var skip = skipTemplate.Invoke(template);
					if (skip)
					{
						Log.Debug($"Skipping {template}");
						continue;
					}
				}

				if (overrideTargetName != null)
				{
					var result = overrideTargetName.Invoke(template);
					if (!string.IsNullOrEmpty(result))
						targetName = result;
				}

				var targetPath = Path.Combine(WorkingDirectory, targetName);
				Log.Debug($"Processing {template}. Target: {targetPath}");

				targetPath.EnsureFolderExists();

				var output = FluentHandlebars.Create(this)
					.WithAllHelpers()
					.HavingModel(Configuration)
					.UsingFileTemplate(templateFile)
					.OutputFile(targetPath, Overwrite);

				templateProcessed?.Invoke(new TemplateProcessed(template, output));
			}
		}
	}

	public class TemplateProcessed
	{
		public TemplateProcessed(string template, string file)
		{
			Template = template;
			File = file;
		}

		public string Template { get; }
		public string File { get; }
	}
}
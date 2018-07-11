using System;
using System.Diagnostics;
using System.IO;
using Genyman.Core.Serializers;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Core.Commands
{
	internal class NewCommand<TConfiguration, TTemplate> : BaseCommand
		where TConfiguration : class
		where TTemplate : TConfiguration, new()
	{
		public NewCommand()
		{
			Name = "new";
			Description = "Generates a template for the generator";
			JsonOption = Option("--json", "Output as json", CommandOptionType.NoValue, option =>
			{
			}, false);

			FileNameOption = Option("--file", "Override filename for template (without extension)", CommandOptionType.SingleOrNoValue, option =>
			{
				
			}, false);
		}

		public CommandOption JsonOption { get; }
		public CommandOption FileNameOption { get; }


		protected override int Execute()
		{
			base.Execute();
			
			var metadata = new GenymanMetadata();
			
			Log.Information($"Executing new command of {metadata.PackageId} - Version {metadata.Version}");
			
			var sw = Stopwatch.StartNew();

			var configuration = new GenymanConfiguration<TConfiguration>
			{
				Genyman = metadata,
				Configuration = new TTemplate(),
			};

			var output = "";
			var extension = "json";

			if (JsonOption.HasValue())
			{
				output = configuration.ToJsonString();
				extension = "json";
			}
			else //Later support more formats!
			{
				// DEFAULT is json
				output = configuration.ToJsonString();
				extension = "json";
			}

			var fileName = !string.IsNullOrEmpty(FileNameOption.Value()) ? FileNameOption.Value() : $"gm-{metadata.Identifier.ToLower()}";
			fileName = $"{fileName}.{extension}";

			var fullFileName = Path.Combine(Environment.CurrentDirectory, fileName);
			if (File.Exists(fileName) && !Overwrite.HasValue())
			{
				Log.Error($"File {fullFileName} already exists. Specify --overwrite if you want to overwrite files");
				return -1;
			}

			File.WriteAllText(fullFileName, output);
			Log.Information($"Configuration file {fileName} was written");
			
			Log.Information($"Finished ({sw.ElapsedMilliseconds}ms)");

			return 0;
		}
	}
}
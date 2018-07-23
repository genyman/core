using System;
using System.Diagnostics;
using System.IO;
using Genyman.Core.Serializers;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Core.Commands
{
	internal class NewCommand : BaseCommand
	{
		protected NewCommand(bool fromCli)
		{
			Name = "new";
			Description = "Generates a confiration file for a generator";
			JsonOption = Option("--json", "Output as json", CommandOptionType.NoValue, option => { }, false);
			FileNameOption = Option<string>("--file", "Override filename for template (without extension)", CommandOptionType.SingleValue, option => { }, false);

			if (fromCli)
			{
				SourceOption = Option<string>("--source", "Custom nuget server location for package", CommandOptionType.SingleValue, option => { }, false);
				UpdateOption = Option("--update", "Perform update for package", CommandOptionType.NoValue, option => { }, false);

				PackageIdArgument = Argument<string>("packageId",
					"Optionally specify a packageId for which you want a new configuration file; if ommitted configuration file for a new generator is created.", argument => { });
			}
		}


		protected CommandOption JsonOption { get; set; }
		protected CommandOption<string> FileNameOption { get; set; }

		protected CommandOption UpdateOption { get; set; }
		protected CommandOption<string> SourceOption { get; set; }
		protected CommandArgument<string> PackageIdArgument { get; set; }

	}

	internal class NewCommand<TConfiguration, TTemplate> : NewCommand
		where TConfiguration : class
		where TTemplate : TConfiguration, new()
	{
		internal Func<int> NewForPackageId { get; set; }

		public NewCommand(bool fromCli) : base(fromCli)
		{
		}

		protected override int Execute()
		{
			base.Execute();

			if (NewForPackageId != null && !string.IsNullOrEmpty(PackageIdArgument.ParsedValue))
			{
				return NewForPackageId();
			}

			var metadata = new GenymanMetadata();
			var version = GetVersion();

			Log.Information($"Executing new command of {metadata.PackageId} - Version {version}");

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

			var fileName = !string.IsNullOrEmpty(FileNameOption.Value()) ? FileNameOption.ParsedValue : $"gm-{metadata.Identifier.ToLower()}";
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
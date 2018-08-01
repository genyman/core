using System;
using System.Diagnostics;
using System.IO;
using Genyman.Core.Serializers;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Core.Commands
{

	internal class NewCommand<TConfiguration, TTemplate> : BaseDispatchCommand
		where TConfiguration : class
		where TTemplate : TConfiguration, new()
	{
		internal Func<int> NewForPackageId { get; set; }

		public NewCommand(bool fromCli) : base(fromCli, "new", "Generates a confiration file for a generator")
		{
			FileNameOption = Option<string>("--file", "Override filename for template (without extension)", CommandOptionType.SingleValue, option => { }, false);
		}

		protected CommandOption<string> FileNameOption { get; set; }

		protected override int Execute()
		{
			var baseResult = base.Execute();
			if (baseResult != 0) return baseResult;

			var metadata = new GenymanMetadata();
			var version = GetVersion();

			Log.Information($"Executing new command for {metadata.PackageId} - Version {version}");

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
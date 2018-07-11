using System;
using System.Diagnostics;
using System.IO;
using Genyman.Core.Serializers;
using McMaster.Extensions.CommandLineUtils;
using ServiceStack;

namespace Genyman.Core.Commands
{
	internal class GenerateCommand<TConfiguration, TGenerator> : BaseCommand
		where TConfiguration : class, new()
		where TGenerator : GenymanGenerator<TConfiguration>, new()
	{
		public GenerateCommand()
		{
			ExtendedHelpText = "\nPowered by Genyman (https://genyman.net)\n";
			

			Input = Argument<string>("input", "File to use for generation", argument => { });
		}

		CommandArgument<string> Input { get; }

		protected override int Execute()
		{
			base.Execute();

			var metaData = new GenymanMetadata();

			if (Input.Value.IsNullOrEmpty())
			{
				Description = $"{metaData.Description} (Version {metaData.Version})";
				Name = metaData.Identifier.ToLower();
				ShowHelp();
				return -1;
			}

			var fileName = Input.Value;

			if (!File.Exists(fileName))
			{
				Log.Error($"Could not find input file {fileName}");
				return -1;
			}

			var configurationContents = File.ReadAllText(fileName);
			GenymanConfiguration<TConfiguration> genyManConfiguration;

			try
			{
				//todo: based upon extension parse this
				genyManConfiguration = configurationContents.FromJsonString<TConfiguration>();
			}
			catch (Exception e)
			{
				Log.Debug(e.Message);
				Log.Error($"Could not parse {fileName} as a valid Genyman input file");
				return -1;
			}

			var generator = new TGenerator
			{
				InputFileName = fileName,
				ConfigurationMetadata = genyManConfiguration.Genyman,
				Configuration = genyManConfiguration.Configuration,
				WorkingDirectory = new FileInfo(fileName).DirectoryName,
				Metadata = metaData,
				Overwrite = Overwrite.HasValue()
			};

			var sw = Stopwatch.StartNew();
			Log.Information($"Executing {generator.Metadata.PackageId} - Version {generator.Metadata.Version}");
			generator.Execute();
			Log.Information($"Finished ({sw.ElapsedMilliseconds}ms)");

			return 0;
		}
	}
}
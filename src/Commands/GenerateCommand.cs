using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Genyman.Core.Serializers;
using McMaster.Extensions.CommandLineUtils;
using ServiceStack;

namespace Genyman.Core.Commands
{
	internal class GenerateCommand<TConfiguration, TGenerator> : BaseCommand
		where TConfiguration : class, new()
		where TGenerator : GenymanGenerator<TConfiguration>, new()
	{
		public GenerateCommand(bool fromCli)
		{
			FromCli = fromCli;
			ExtendedHelpText = "\nPowered by Genyman (https://genyman.github.io/docs)\n";
			Input = Argument<string>("input", "File to use for generation", argument => { });
			if (FromCli)
			{
				UpdateOption = Option("--update", "Perform update for package", CommandOptionType.NoValue, option => { }, false);
			}
		}

		CommandArgument<string> Input { get; }
		internal CommandOption UpdateOption { get; }
		internal bool FromCli { get; }

		protected override int Execute()
		{
			base.Execute();

			var metaData = new GenymanMetadata();
			var version = GetVersion();

			if (Input.Value.IsNullOrEmpty())
			{
				Description = $"{metaData.Description} (Version {version})";
				Name = metaData.Identifier.ToLower();
				ShowHelp();
				return -1;
			}

			var fileName = Input.ParsedValue;

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
				Overwrite = Overwrite.HasValue(),
				Update = UpdateOption?.HasValue() ?? false
			};
			
			var sw = Stopwatch.StartNew();
			Log.Information($"Executing {generator.Metadata.PackageId} - Version {version}");
			generator.Execute();
			var elapsed = sw.ElapsedMilliseconds;
			Log.Information($"Finished {generator.Metadata.PackageId} ({elapsed}ms)");

			// telemetry
			if (metaData.PackageId.ToLower() != "genyman")
			{
				const string telemetryUrl = "https://cmgg8m1ib1.execute-api.us-east-2.amazonaws.com/genyman";
				telemetryUrl.PostJsonToUrl(new Telemetry() {Name = metaData.PackageId, Version = metaData.Version, Duration = elapsed});
			}

			return 0;
		}

		

		class Telemetry
		{
			public string Name { get; set; }
			public string Version { get; set; }
			public long Duration { get; set; }
		}
	}
}
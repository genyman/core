using System.IO;
using System.Runtime.InteropServices;
using Genyman.Core.Helpers;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Core.Commands
{
	public class BaseDispatchCommand : BaseCommand
	{
		protected BaseDispatchCommand(bool fromCli, string command, string description)
		{
			FromCli = fromCli;
			
			Name = command;
			
			Description = description;
			JsonOption = Option("--json", "Output for json", CommandOptionType.NoValue, option => { }, false);

			if (FromCli)
			{
				SourceOption = Option<string>("--source", "Custom nuget server location for package", CommandOptionType.SingleValue, option => { }, false);
				UpdateOption = Option("--update", "Perform update for package", CommandOptionType.NoValue, option => { }, false);
				
				PackageIdArgument = Argument<string>("packageId",
					"Optionally specify a packageId to run the command for; if ommitted command is for Genyman generator", argument => { });
			}
		}
		
		internal bool FromCli { get; set; }
		
		protected CommandOption JsonOption { get; set; }
		
		protected CommandOption UpdateOption { get; set; }
		protected CommandOption<string> SourceOption { get; set; }
		
		internal CommandArgument<string> PackageIdArgument { get; set; }
		
		protected override int Execute()
		{
			base.Execute();

			if (FromCli && !string.IsNullOrEmpty(PackageIdArgument.ParsedValue))
			{
				var packageId = PackageIdArgument.ParsedValue;
				var resolvePackageResult = DotNetRunner.ResolvePackage(packageId, SourceOption.ParsedValue, UpdateOption.HasValue(), string.Empty);

				if (resolvePackageResult.success)
				{
					var program = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
						? Path.Combine(DotNetRunner.CliFolderPathCalculator.ToolsShimPath, resolvePackageResult.packageId)
						: resolvePackageResult.packageId;

					var run = ProcessRunner.Create(program)
						.IsGenerator()
						.WithArgument(Name);

					foreach (var option in Options)
						if (option.HasValue())
							run.WithArgument("--" + option.LongName, option.Value());

					run.Execute(false);
					return 1;
				}

				Log.Error($"Could not execute {Name} command for this packageId.");
				return -1;
			}
			
			return 0;
		}
	}
}
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Core.Commands
{
	public abstract class BaseCommand : CommandLineApplication
	{
		protected BaseCommand() : base(false)
		{
			Overwrite = Option("--overwrite", "Overwrite files if they exist", CommandOptionType.NoValue, option => { }, true);
			Diagnostic = Option("--diagnostic", "Output diagnostic logging", CommandOptionType.NoValue, option => { }, true);
			Quiet = Option("--quiet", "Do not Output any logging", CommandOptionType.NoValue, option => { }, true);

			Invoke = Execute;
			HelpOption("--help");
		}

		protected CommandOption Overwrite { get; }
		protected CommandOption Diagnostic { get; }
		protected CommandOption Quiet { get; }


		protected virtual int Execute()
		{
			if (Diagnostic.HasValue()) Log.Verbosity = Verbosity.Diagnostic;
			if (Quiet.HasValue()) Log.Verbosity = Verbosity.Quiet;
			return 0;
		}
	}
}
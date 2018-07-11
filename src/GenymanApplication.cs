using System;
using System.Collections.Generic;
using Genyman.Core.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Core
{
	public class GenymanApplication
	{
		public static void Run<TConfiguration, TTemplate, TGenerator>(string[] args)
			where TConfiguration : class, new()
			where TTemplate : TConfiguration, new()
			where TGenerator : GenymanGenerator<TConfiguration>, new()
		{
			Run<TConfiguration, TTemplate, TGenerator>(args, null);
		}

		internal static void Run<TConfiguration, TTemplate, TGenerator>(string[] args, Action<List<CommandLineApplication>> subCommands)
			where TConfiguration : class, new()
			where TTemplate : TConfiguration, new()
			where TGenerator : GenymanGenerator<TConfiguration>, new()
		{
			// setup default generate command
			var generateCommand = new GenerateCommand<TConfiguration, TGenerator>();
			generateCommand.Conventions.UseDefaultConventions();
			var newCommand = new NewCommand<TConfiguration, TTemplate>();
			generateCommand.Commands.Add(newCommand);
			subCommands?.Invoke(generateCommand.Commands);
			try
			{
				generateCommand.Execute(args);
			}
			catch (CommandParsingException e)
			{
				Log.Debug(e.ToString());
				generateCommand.ShowHelp();
			}
			catch (Exception e)
			{
				Log.Debug(e.ToString());
				Log.Fatal($"Fatal error; something went wrong.");
			}
		}
	}
}
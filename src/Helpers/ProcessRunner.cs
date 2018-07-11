using System;
using System.Collections.Generic;
using System.Diagnostics;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Core.Helpers
{
	public class ProcessRunner
	{
		static ProcessRunner _processRunner;
		readonly List<string> _arguments = new List<string>();
		Func<string, bool> _receiveOutput;
		readonly string _processPath = "";

		ProcessRunner(string processPath)
		{
			_processPath = processPath;
		}

		public static ProcessRunner Create(string processPath)
		{
			_processRunner = new ProcessRunner(processPath);
			return _processRunner;
		}

		public ProcessRunner WithArgument(string argument, string value = null)
		{
			_arguments.Add($"{argument}");
			if (value != null)
				_arguments.Add(value);
			return _processRunner;
		}

		public ProcessRunner ReceiveOutput(Func<string, bool> receiveOutput)
		{
			_receiveOutput = receiveOutput;
			return _processRunner;
		}

		public void Execute()
		{
			var arguments = ArgumentEscaper.EscapeAndConcatenate(_arguments);

			var processStartInfo = new ProcessStartInfo
			{
				UseShellExecute = false,
				FileName = _processPath,
				Arguments = arguments,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			Log.Debug($"Executing {_processPath}");
			Log.Debug($"Arguments: {arguments}");

			var process = new Process
			{
				StartInfo = processStartInfo
			};

			try
			{
				process.Start();
				process.WaitForExit();
			}
			catch (Exception e)
			{
				Log.Debug(e.ToString());
				Log.Error(e.Message);

				Log.Fatal($"Error executing process {_processPath}");
			}

			if (process.ExitCode == 0)
				RedirectOutput(process, false, _receiveOutput);
			else
				RedirectOutput(process, true, _receiveOutput);
		}

		static void RedirectOutput(Process process, bool asError, Func<string, bool> receiveOutput = null)
		{
			var output = process.StandardOutput.ReadToEnd();
			var error = process.StandardError.ReadToEnd();

			var logOutput = true;
			if (receiveOutput != null)
			{
				logOutput = receiveOutput.Invoke(output);
			}

			if (logOutput)
			{
				if (!asError)
					Log.Debug(output);
				else
					Log.Error(output);

				if (!string.IsNullOrEmpty(error))
					Log.Error(error);
			}
		}
	}
}
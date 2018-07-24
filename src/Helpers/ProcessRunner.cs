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
		bool _isGenerator = false;
		bool _useShellExecute = false;
		string _pathEnvironmentVariable = null;

		ProcessRunner(string processPath)
		{
			_processPath = processPath;
		}

		public static ProcessRunner Create(string processPath)
		{
			_processRunner = new ProcessRunner(processPath);
			return _processRunner;
		}

		internal ProcessRunner IsGenerator()
		{
			_isGenerator = true;
			return _processRunner;
		}

		public ProcessRunner WithArgument(string argument, string value = null)
		{
			_arguments.Add($"{argument}");
			if (value != null)
				_arguments.Add(value);
			return _processRunner;
		}

		public ProcessRunner WithUseShellExecute()
		{
			_useShellExecute = true;
			return _processRunner;
		}

		public ProcessRunner WithPathEnvironmentVariable(string pathEnvironmentVariable)
		{
			_pathEnvironmentVariable = pathEnvironmentVariable;
			return _processRunner;
		}

		public ProcessRunner ReceiveOutput(Func<string, bool> receiveOutput)
		{
			_receiveOutput = receiveOutput;
			return _processRunner;
		}

		public int Execute(bool redirectOutput)
		{
			var arguments = ArgumentEscaper.EscapeAndConcatenate(_arguments);

			var processStartInfo = new ProcessStartInfo
			{
				UseShellExecute = _useShellExecute,
				FileName = _processPath,
				Arguments = arguments,
				RedirectStandardOutput = redirectOutput,
				RedirectStandardError = redirectOutput,
			};

			if (!string.IsNullOrEmpty(_pathEnvironmentVariable))
			{
				if (processStartInfo.EnvironmentVariables.ContainsKey("PATH"))
					processStartInfo.EnvironmentVariables["PATH"] = _pathEnvironmentVariable;
				else
					processStartInfo.EnvironmentVariables.Add("PATH", _pathEnvironmentVariable);
			}

			var pathVariable = processStartInfo.EnvironmentVariables.ContainsKey("PATH") ? processStartInfo.EnvironmentVariables["PATH"] : "No PATH environment variable found";

			Log.Debug($"Executing {_processPath}");
			Log.Debug($"Arguments: {arguments}");
			Log.Debug($"Path: {pathVariable}");

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
				RedirectOutput(redirectOutput, process, false, _isGenerator, _receiveOutput);
			else
				RedirectOutput(redirectOutput, process, true, _isGenerator, _receiveOutput);

			return process.ExitCode;
		}

		static void RedirectOutput(bool redirectOutput, Process process, bool asError, bool isGenerator, Func<string, bool> receiveOutput = null)
		{
			if (!redirectOutput) return;

			var output = process.StandardOutput.ReadToEnd();
			var error = process.StandardError.ReadToEnd();

			if (isGenerator)
			{
				Log.FromGenerator(output, error);
			}
			else
			{
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
}
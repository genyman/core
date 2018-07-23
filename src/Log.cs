using System;

namespace Genyman.Core
{
	public static class Log
	{

		public static void Debug(string message)
		{
			InternalWrite(LogLevel.Debug, message);
		}

		public static void Information(string message)
		{
			InternalWrite(LogLevel.Information, message);
		}

		public static void Warning(string message)
		{
			InternalWrite(LogLevel.Warning, message);
		}

		public static void Error(string message)
		{
			InternalWrite(LogLevel.Error, message);
		}

		public static void Fatal(string message)
		{
			InternalWrite(LogLevel.Fatal, message);
		}

		public static Verbosity Verbosity { get; set; } = Verbosity.Normal;

		static void InternalWrite(LogLevel level, string message)
		{
			if (Verbosity == Verbosity.Quiet) return;
			if (Verbosity == Verbosity.Normal && level == LogLevel.Debug) return;
			
			var messages = message.Split(Environment.NewLine);
			foreach (var line in messages)
			{
				if (string.IsNullOrEmpty(line)) continue;
				
				var prefix = $"{LevelPrefix(level)}: ";
				Console.WriteLine($"{prefix}{line}");
				if (level == LogLevel.Fatal)
					Environment.Exit(-1);
			}
		}

		internal static void FromGenerator(string messages, string errors)
		{
			var lines = messages.Split(Environment.NewLine);
			foreach (var line in lines)
			{
				if (string.IsNullOrEmpty(line)) continue;
				Console.WriteLine(line);
			}
			
			var errorLines = errors.Split(Environment.NewLine);
			foreach (var line in errorLines)
			{
				if (string.IsNullOrEmpty(line)) continue;
				Console.WriteLine(line);
			}
		}
			

		static string LevelPrefix(LogLevel logLevel)
		{
			
			switch (logLevel)
			{
				case LogLevel.Fatal:
					return "FTL";
				case LogLevel.Error:
					return "ERR";
				case LogLevel.Warning:
					return "WRN";
				case LogLevel.Information:
					return "INF";
				case LogLevel.Debug:
					return "DBG";
			}

			return null;
		}

	}

	public enum Verbosity
	{
		Normal,
		Quiet,
		Diagnostic
	}

	public enum LogLevel
	{
		Fatal,
		Error,
		Warning,
		Information,
		Debug
	}
}
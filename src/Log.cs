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
				var prefix = $"{LevelPrefix(level, message)}: ";
				Console.WriteLine($"{prefix}{line}");
				if (level == LogLevel.Fatal)
					Environment.Exit(-1);
			}
		}

		static string LevelPrefix(LogLevel logLevel, string message)
		{
			if (message.StartsWith("FTL") || message.StartsWith("ERR") || message.StartsWith("WRN") || message.StartsWith("INF") || message.StartsWith("DBG"))
				return message;
			
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
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
				var prefix = string.Empty;
				if (Verbosity == Verbosity.Diagnostic)
					prefix = $"[{LevelPrefix(level)}] ";
				switch (level)
				{
					case LogLevel.Fatal:
						Console.ForegroundColor = ConsoleColor.DarkRed;
						break;
					case LogLevel.Error:
						Console.ForegroundColor = ConsoleColor.Red;
						break;
					case LogLevel.Warning:
						Console.ForegroundColor = ConsoleColor.Yellow;
						break;
					case LogLevel.Information:
						break;
					case LogLevel.Debug:
						Console.ForegroundColor = ConsoleColor.DarkGray;
						break;
				}

				Console.WriteLine($"{prefix}{line}");
				Console.ResetColor();
				if (level == LogLevel.Fatal)
					Environment.Exit(-1);
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
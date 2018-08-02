using System;

// ReSharper disable once CheckNamespace
namespace Genyman.Core
{
	public static class EnvironmentHelpers
	{
		public static string FromEnvironmentOrDefault(this string value)
		{
			if (value.StartsWith("$")) return Environment.GetEnvironmentVariable(value.Replace("$",""));
			return value;
		}
	}
}
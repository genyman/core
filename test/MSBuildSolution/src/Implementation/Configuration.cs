using System;
using Genyman.Core;

namespace Sample.Genyman.MSBuildSolution.Implementation
{
	[Documentation()]
	public class Configuration
	{
		public bool UseIOS { get; set; }
		public bool UseAndroid { get; set; }
	}
}
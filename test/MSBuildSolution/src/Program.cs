using Sample.Genyman.MSBuildSolution.Implementation;
using Genyman.Core;

namespace Sample.Genyman.MSBuildSolution
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			GenymanApplication.Run<Configuration, NewTemplate, Generator>(args);
		}
	}
}
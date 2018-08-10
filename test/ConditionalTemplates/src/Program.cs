using Sample.Genyman.ConditionalTemplates.Implementation;
using Genyman.Core;

namespace Sample.Genyman.ConditionalTemplates
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			GenymanApplication.Run<Configuration, NewTemplate, Generator>(args);
		}
	}
}
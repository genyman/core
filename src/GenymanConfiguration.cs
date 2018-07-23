namespace Genyman.Core
{
	public class GenymanConfiguration<T>
	{
		public GenymanMetadata Genyman { get; set; }
		public T Configuration { get; set; }
	}
}
namespace Genyman.Core.Serializers
{
	internal static class XmlSerializer
	{
		internal static GenymanConfiguration<T> FromXmlString<T>(this string xml) where T : new()
		{
			//todo
			return null;
		}

		internal static string ToXmlString(this object config)
		{
			//todo
			return $"Currently not supported - PR's welcome! (https://genyman.net)";
		}
	}
}
namespace Genyman.Core.Serializers
{
	internal static class YamlSerializer
	{
		internal static GenymanConfiguration<T> FromYamlString<T>(this string xml) where T : new()
		{
			//todo
			return null;
		}

		internal static string ToYamlString(this object config)
		{
			//todo
			return $"Currently not supported - PR's welcome! (https://genyman.net)";
		}
	}
}
using ServiceStack;
using ServiceStack.Text;

namespace Genyman.Core.Serializers
{
	internal static class JsonSerializer
	{
		internal static GenymanConfiguration<T> FromJsonString<T>(this string json) where T : new()
		{
			var configuration = json.FromJson<GenymanConfiguration<T>>();
			return configuration;
		}

		internal static string ToJsonString(this object config)
		{
			JsConfig.ExcludeTypeInfo = true;
			JsConfig.EmitCamelCaseNames = true;
			JsConfig.IgnoreAttributesNamed = new[] {nameof(GenymanIgnoreAttribute)};
			return config.ToJson().IndentJson();
		}
	}
}
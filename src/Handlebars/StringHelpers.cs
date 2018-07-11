using HandlebarsDotNet;

namespace Genyman.Core.Handlebars
{
	internal static class StringHelpers
	{
		internal static void Init(IHandlebars handlebars)
		{
			handlebars.RegisterHelper("append", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString() || !parameters.MustBeString(1)) return;
				writer.WriteSafeString(Concat(parameters[0].ToString(), parameters[1].ToString()));
			});

			handlebars.RegisterHelper("prepend", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString() || !parameters.MustBeString(1)) return;
				writer.WriteSafeString(ConcatReverse(parameters[0].ToString(), parameters[1].ToString()));
			});

			handlebars.RegisterHelper("remove", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString() || !parameters.MustBeString(1)) return;
				writer.WriteSafeString(Remove(parameters[0].ToString(), parameters[1].ToString()));
			});

			handlebars.RegisterHelper("replace", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString() || !parameters.MustBeString(1) || !parameters.MustBeString(2)) return;
				writer.WriteSafeString(Replace(parameters[0].ToString(), parameters[1].ToString(), parameters[2].ToString()));
			});

			handlebars.RegisterHelper("lower", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString()) return;
				writer.WriteSafeString(ToLower(parameters[0].ToString()));
			});

			handlebars.RegisterHelper("upper", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString()) return;
				writer.WriteSafeString(ToUpper(parameters[0].ToString()));
			});

			handlebars.RegisterHelper("camelcase", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString()) return;
				writer.WriteSafeString(ToCamelCase(parameters[0].ToString()));
			});

			handlebars.RegisterHelper("pascalcase", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString()) return;
				writer.WriteSafeString(ToPascalCase(parameters[0].ToString()));
			});

			handlebars.RegisterHelper("capitalize", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString()) return;
				writer.WriteSafeString(Capitalize(parameters[0].ToString()));
			});
		}

		static string ToLower(string value) => value.ToLower();
		static string ToUpper(string value) => value.ToUpper();
		static string Concat(string value1, string value2) => string.Concat(value1, value2);
		static string ConcatReverse(string value1, string value2) => string.Concat(value2, value1);
		static string Remove(string value1, string value2) => value1.Replace(value2, "");
		static string Replace(string value1, string value2, string value3) => value1.Replace(value2, value3);

		internal static string ToCamelCase(string value)
		{
			var parts = value.Split(' ');
			var result = parts[0].Substring(0, 1).ToLower() + parts[0].Substring(1);
			for (int i = 1; i < parts.Length; i++)
			{
				result += parts[i].Substring(0, 1).ToUpper() + parts[i].Substring(1);
			}
			return result;
		}

		static string Capitalize(string value)
		{
			var parts = value.Split(' ');
			var result = parts[0].Substring(0, 1).ToUpper() + parts[0].Substring(1);
			for (var i = 1; i < parts.Length; i++)
			{
				result += " " + parts[i];
			}
			return result;
		}

		static string ToPascalCase(string value)
		{
			var parts = value.Split(' ');
			var result = string.Empty;
			for (var i = 0; i < parts.Length; i++)
			{
				result += parts[i].Substring(0, 1).ToUpper() + parts[i].Substring(1);
			}
			return result;
		}
	}
}
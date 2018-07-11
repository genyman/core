using HandlebarsDotNet;

namespace Genyman.Core.Handlebars
{
	internal class CSharpHelpers
	{
		internal static void Init(IHandlebars handlebars)
		{
			handlebars.RegisterHelper("csharp-membervar", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString()) return;
				writer.WriteSafeString(ToMemberVariable(parameters[0].ToString()));
			});

			handlebars.RegisterHelper("csharp-type", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString()) return;
				if (!parameters.MustBeBooleanOrNull(1)) return;
				writer.WriteSafeString(parameters.Length == 1 
					? ToType(parameters[0].ToString()) 
					: ToType(parameters[0].ToString(), (bool) parameters[1]));
			});
		}

		static string ToMemberVariable(string value)
		{
			return string.Concat("_", StringHelpers.ToCamelCase(value));
		}

		static string ToType(string type, bool nullable = false)
		{
			switch(type.ToLower())
			{
				case "bool":
				case "byte":
				case "char":
				case "decimal":
				case "double":
				case "float":
				case "int":
				case "long":
				case "sbyte":
				case "short":
				case "uint":
				case "ulong":
				case "ushort":
					return type + (nullable ? "?" : "");
				default:
					return type;
			}
		}
	}

}
using System.Linq;
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
				var variable = parameters[0].ToString().Split('.').Last();
				writer.WriteSafeString(ToMemberVariable(variable));
			});
			
			handlebars.RegisterHelper("csharp-property", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString()) return;
				var variable = parameters[0].ToString().Split('.').Last();
				writer.WriteSafeString(StringHelpers.ToPascalCase(variable));
			});

			handlebars.RegisterHelper("csharp-var", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString()) return;
				var variable = parameters[0].ToString().Split('.').Last();
				writer.WriteSafeString(StringHelpers.ToCamelCase(variable));
			});
			
			handlebars.RegisterHelper("csharp-type", (writer, context, parameters) =>
			{
				if (!parameters.MustBeString()) return;
				if (!parameters.MustBeBooleanOrNull(1)) return;
				writer.WriteSafeString(parameters.Length == 1 
					? ToType(parameters[0].ToString()) 
					: ToType(parameters[0].ToString(), (bool) parameters[1]));
			});
			
			handlebars.RegisterHelper("csharp-parameters", (writer, context, parameters) =>
			{
				if (!parameters.MustBeStringArray()) return;
				var items = parameters[0] as string[];
				var output = string.Empty;
				foreach (var item in items)
				{
					var variable = item.Split('.').Last();
					output += $"{item} {StringHelpers.ToCamelCase(variable)}, ";
				}
				output = output.Substring(0, output.Length - 2);
				writer.WriteSafeString(output);
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
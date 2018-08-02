using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Genyman.Core.Serializers;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Core.Commands
{
	internal class DocCommand<TConfiguration, TTemplate> : BaseDispatchCommand
		where TConfiguration : class
		where TTemplate : TConfiguration, new()
	{
		public DocCommand(bool fromCli) : base(fromCli, "doc", "Shows documentation for the configuration file")
		{
			MarkdownOption = Option<string>("--md", "Generates Markdown file for the given filename", CommandOptionType.SingleValue, option => { }, false);
		}

		protected CommandOption<string> MarkdownOption { get; set; }


		protected override int Execute()
		{
			var baseResult = base.Execute();
			if (baseResult != 0) return baseResult;

			var metadata = new GenymanMetadata();
			var version = GetVersion();

			Log.Information($"Executing doc command for {metadata.PackageId} - Version {version}");

			var markdown = new List<string>();
			var useMarkdown = MarkdownOption.HasValue();

			if (useMarkdown)
			{
				markdown.Add($"# {metadata.PackageId}");
				markdown.Add(metadata.Description);
				markdown.Add("## Getting Started");
				markdown.Add($"{metadata.PackageId} is a **[genyman](http://genyman.net)** code generator. If you haven't installed **genyman** run following command:");
				markdown.Add($"```");
				markdown.Add($"dotnet tool install -g genyman");
				markdown.Add($"```");
				markdown.Add($"_Genyman is a .NET Core Global tool and thereby you need .NET Core version 2.1 installed._");

				markdown.Add("## New Configuration file ");
				markdown.Add($"```");
				if (metadata.Identifier == "Genyman")
					markdown.Add($"genyman new");
				else
					markdown.Add($"genyman new {metadata.PackageId}");
				markdown.Add($"```");

				markdown.Add("## Sample Configuration file ");

				var configuration = new GenymanConfiguration<TConfiguration>
				{
					Genyman = metadata,
					Configuration = new TTemplate(),
				};
				var output = configuration.ToJsonString();
				markdown.Add($"```");
				markdown.Add(output);
				markdown.Add($"```");

				markdown.Add("## Documentation ");
			}

			var types = typeof(TConfiguration).Assembly.GetExportedTypes();
			foreach (var type in types)
			{
				var documentation = type.GetCustomAttribute<DocumentationAttribute>();
				if (documentation == null) continue;

				if (type.IsClass)
				{

					
					var output = new List<PropertyList>();

					var properties = type.GetProperties();
					foreach (var property in properties)
					{
						var ignoreAttribute = property.GetCustomAttribute<IgnoreAttribute>();
						if (ignoreAttribute != null) continue;

						var descriptionAttribute = property.GetCustomAttribute<DescriptionAttribute>();
						var requiredAttribute = property.GetCustomAttribute<RequiredAttribute>();

						var typeName = property.PropertyType.Name;
						if (property.PropertyType.IsEnum)
							typeName += " (Enum)";
						else if (property.PropertyType.IsGenericType && typeName.Contains("List"))
							typeName = $"{property.PropertyType.GenericTypeArguments[0].Name}[]";


						output.Add(new PropertyList()
						{
							Name = property.Name,
							Type = typeName,
							Required = requiredAttribute != null ? "*" : "",
							Description = descriptionAttribute?.Description
						});
					}

					WriteHeader(type);

					var table = new PrintableTable<PropertyList>();
					table.AddColumn("Name", p => p.Name);
					table.AddColumn("Type", p => p.Type);
					table.AddColumn("Req", p => p.Required);
					table.AddColumn("Description", p => p.Description);
					table.PrintRows(output, Console.WriteLine);
					
					if (useMarkdown)
					{
						markdown.Add($"### Class {type.Name}");
						if(!string.IsNullOrEmpty(documentation.Remarks))
							markdown.Add(documentation.Remarks);
						
						markdown.Add($"| Name | Type | Req | Description |");
						markdown.Add($"| --- | --- | :---: | --- |");
						foreach (var p in output)
						{
							markdown.Add($"| {p.Name} | {p.Type} | {p.Required} | {p.Description} |");
						}
					}
				}
				else if (type.IsEnum)
				{
					var output = new List<EnumList>();

					var fields = type.GetFields();
					foreach (var fieldInfo in fields)
					{
						if (fieldInfo.IsSpecialName) continue;
						var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
						output.Add(new EnumList()
						{
							Name = fieldInfo.Name,
							Description = descriptionAttribute?.Description
						});
					}

					WriteHeader(type);

					var table = new PrintableTable<EnumList>();
					table.AddColumn("Name", p => p.Name);
					table.AddColumn("Description", p => p.Description);
					table.PrintRows(output, Console.WriteLine);
					
					if (useMarkdown)
					{
						markdown.Add($"### Enum {type.Name}");
						if(!string.IsNullOrEmpty(documentation.Remarks))
							markdown.Add(documentation.Remarks);
						
						markdown.Add($"| Name | Description |");
						markdown.Add($"| --- | --- |");
						foreach (var p in output)
						{
							markdown.Add($"| {p.Name} | {p.Description} |");
						}
					}
				}
			}

			if (useMarkdown)
			{
				var target = Path.Combine(WorkingDirectory, MarkdownOption.ParsedValue);
				target.EnsureFolderExists();
				File.WriteAllLines(target, markdown);
			}

			return 0;
		}

		static void WriteHeader(Type type)
		{
			var typeText = type.IsClass ? "Class" : type.IsEnum ? "Enum" : "?";
			var header = $"{typeText}: {type.Name}";
			var documentationAttribute = type.GetCustomAttribute<DocumentationAttribute>();

			Console.WriteLine();
			Console.WriteLine(header);
			Console.WriteLine(new string('-', header.Length));
			Console.WriteLine();
			if (!string.IsNullOrEmpty(documentationAttribute?.Remarks))
			{
				Console.WriteLine(documentationAttribute.Remarks);
				Console.WriteLine();
			}
			if (!string.IsNullOrEmpty(documentationAttribute?.Source))
			{
				Console.WriteLine($"Source: {documentationAttribute.Source}");
				Console.WriteLine();
			}
		}


		public class PropertyList
		{
			public string Name { get; set; }
			public string Type { get; set; }
			public string Description { get; set; }
			public string Required { get; set; }
		}

		public class EnumList
		{
			public string Name { get; set; }
			public string Description { get; set; }
		}
	}
}
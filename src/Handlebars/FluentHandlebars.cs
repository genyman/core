using System;
using System.IO;
using System.Reflection;

namespace Genyman.Core.Handlebars
{
	public class FluentHandlebars
	{
		static FluentHandlebars _fluentInstance;
		readonly HandlebarsDotNet.IHandlebars _handlebars;
		string _template;
		object _model;
		readonly object _caller;

		FluentHandlebars(object caller)
		{
			_handlebars = HandlebarsDotNet.Handlebars.Create();
			_caller = caller;
		}

		public static FluentHandlebars Create(object caller)
		{
			return _fluentInstance = new FluentHandlebars(caller);
		}

		public HandlebarsDotNet.IHandlebars Instance => _handlebars;

		public FluentHandlebars WithStringHelpers()
		{
			StringHelpers.Init(_handlebars);
			return _fluentInstance;
		}

		public FluentHandlebars WithCSharpHelpers()
		{
			CSharpHelpers.Init(_handlebars);
			return _fluentInstance;
		}

		public FluentHandlebars WithAllHelpers()
		{
			_fluentInstance.WithStringHelpers();
			_fluentInstance.WithCSharpHelpers();
			return _fluentInstance;
		}

		public FluentHandlebars UsingTemplate(string template)
		{
			_template = template;
			return _fluentInstance;
		}

		public FluentHandlebars UsingEmbeddedTemplate(string embeddedResource)
		{
			var assembly = _caller.GetType().GetTypeInfo().Assembly;

			string source;
			var stream = assembly.GetManifestResourceStream(embeddedResource);
			if (stream == null)
			{
				var allResources = assembly.GetManifestResourceNames();
				foreach (var resource in allResources)
				{
					if (resource.EndsWith(embeddedResource, StringComparison.CurrentCultureIgnoreCase))
						stream = assembly.GetManifestResourceStream(resource);
				}
			}
			using (var reader = new StreamReader(stream))
			{
				source = reader.ReadToEnd();
			}
			_template = source;
			return _fluentInstance;
		}

		public FluentHandlebars UsingFileTemplate(string fileName)
		{
			_template = File.ReadAllText(fileName);
			return _fluentInstance;
		}

		public FluentHandlebars HavingModel<T>(T model) where T : class
		{
			_model = model;
			return _fluentInstance;
		}

		public string OutputString()
		{
			var template = _handlebars.Compile(_template);
			return template(_model);
		}

		public bool OutputFile(string fileName, bool overwrite = false)
		{
			if (fileName.Contains("{{"))
			{
				fileName = FluentHandlebars.Create(this)
					.HavingModel(_model)
					.UsingTemplate(fileName)
					.OutputString();
			}
			
			var result = OutputString();
			if (!overwrite && File.Exists(fileName))
			{
				Log.Warning($"Skipping {fileName} - File already exists");
				return false;
			}
			try
			{
				File.WriteAllText(fileName, result, System.Text.Encoding.UTF8);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
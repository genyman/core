﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ServiceStack;

namespace Genyman.Core.Handlebars
{
	public class FluentHandlebars
	{
		static FluentHandlebars _fluentInstance;
		readonly HandlebarsDotNet.IHandlebars _handlebars;
		string _template;
		object _model;
		readonly object _caller;

		public static readonly List<string> DefaultSkippedExtensions = new List<string>()
		{
			"png", "jpg", "jpeg", "gif", "bmp", "tif", "tiff",
			"dll", "bin", "pbd"
		};

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

		internal bool Skip { get; set; }

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

		public FluentHandlebars WithCustomHelper(Action<HandlebarsDotNet.IHandlebars> customHelper)
		{
			customHelper.Invoke(_handlebars);
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
			if (DefaultSkippedExtensions.Contains(fileName.GetExtension().Replace(".", "")))
			{
				_fluentInstance.Skip = true;
				Log.Debug($"{fileName.GetExtension()} cannot be handled by Handlebars");
			}
			else
			{
				try
				{
					_template = File.ReadAllText(fileName);
				}
				catch (Exception)
				{
					Skip = true;
					Log.Error($"Could not read {fileName}. Is this a text file? If not, you can add the extension to FluentHandlebars.DefaultSkippedExtensions list");
				}
			}

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

		public string OutputFile(string fileName, bool overwrite = false)
		{
			if (Skip) return null;

			if (fileName.Contains("{{"))
			{
				// trick handlebars on Windows file names
				var escapeAvailable = false;
				if (fileName.Contains("\\"))
				{
					escapeAvailable = true;
					fileName = fileName.Replace("\\", "/");
				}

				fileName = FluentHandlebars.Create(this)
					.HavingModel(_model)
					.UsingTemplate(fileName)
					.OutputString();

				if (escapeAvailable)
					fileName = fileName.Replace("/", "\\");
			}

			var result = OutputString();
			if (!overwrite && File.Exists(fileName))
			{
				Log.Warning($"Skipping {fileName} - File already exists");
				return fileName;
			}

			try
			{
				fileName.EnsureFolderExists();
				File.WriteAllText(fileName, result, System.Text.Encoding.UTF8);
				return fileName;
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
				throw;
			}
		}
	}
}
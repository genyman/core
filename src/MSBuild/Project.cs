using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Genyman.Core.MSBuild
{
	public class Project
	{
		internal const string DotNetXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

		string _fileName;
		protected Solution _solution;
		protected List<string> _contents;

		internal Project()
		{
		}

		protected string Name { get; set; }

		public string FileName
		{
			get => _fileName;
			internal set => _fileName = value.ToPlatformPath();
		}

		public bool Loaded { get; internal set; }
		public Exception Exception { get; internal set; }
		public ProjectType Type { get; internal set; }
		public ProjectSubType SubType { get; internal set; }
		public string TargetFramework { get; internal set; }
		public string AssemblyName { get; internal set; }
		public string RootNamespace { get; internal set; }
		public string OutputType { get; internal set; }
		protected bool IsSharedProject { get; set; }

		public Solution Solution
		{
			get
			{
				if (_solution != null)
					return _solution;
				_solution = Solution.LoadFromFile(FileName);
				return _solution;
			}
		}

		public virtual void Load()
		{
			if (Loaded) return;

			try
			{
				if (!File.Exists(FileName))
					throw new FileNotFoundException();

				var fileInfo = new FileInfo(FileName);
				Name = fileInfo.Name.Replace(fileInfo.Extension, "");
				FileName = fileInfo.FullName;
				ProjectDirectory = fileInfo.DirectoryName;

				var types = DetermineProjectType(File.ReadAllText(FileName));
				Type = types.Item1;
				SubType = types.Item2;

				_contents = System.IO.File.ReadAllLines(FileName).ToList();

				RootNamespace = FindProperty("RootNamespace");
				OutputType = FindProperty("OutputType");
				AssemblyName = FindProperty("AssemblyName");

				if (Type == ProjectType.DotNet)
				{
					TargetFramework = FindProperty("TargetFrameworkVersion");

					if (SubType == ProjectSubType.XamarinAndroid)
					{
						var isApplication = FindProperty("AndroidApplication");
						if (isApplication == "True")
							OutputType = "Exe";
					}
				}
				else
				{
					TargetFramework = FindProperty("TargetFramework");

					if (string.IsNullOrEmpty(RootNamespace))
						RootNamespace = Name;
					if (string.IsNullOrEmpty(AssemblyName))
						AssemblyName = Name;
					if (string.IsNullOrEmpty(OutputType))
						OutputType = SubType == ProjectSubType.Library ? "Library" : "Exe";
				}

				Loaded = true;
			}
			catch (Exception e)
			{
				Exception = e;
			}
		}

		internal bool AddFile(string target, BuildAction buildAction, string dependentUpon = null, string customTool = null)
		{
			var resultPath = target.Replace(ProjectDirectory, "").Substring(1).ToMSBuildPath(); // remove first / in path
			if (IsSharedProject)
			{
				resultPath = $"$(MSBuildThisFileDirectory){resultPath}";
			}
			if (!string.IsNullOrEmpty(dependentUpon))
				dependentUpon = dependentUpon.Replace(ProjectDirectory, "").Substring(1).ToMSBuildPath();

			if (Type == ProjectType.DotNet)
			{
				// find this type
				var write = false;
				var lastItemGroupIndex = 0;
				var processed = false;
				for (var i = 0; i < _contents.Count; i++)
				{
					if (_contents[i].Trim() != "<ItemGroup>") continue;
					lastItemGroupIndex = i;


					if (_contents[i + 1].Trim().StartsWith("<" + buildAction))
					{
						var needsToAdd = true;

						// found; do while

						do
						{
							i++;
							if (_contents[i].Contains(resultPath))
							{
								needsToAdd = false;
								processed = true;
								break;
							}
						} while (_contents[i + 1].Trim() != "</ItemGroup>");

						if (needsToAdd)
						{
							i++;
							AddItemGroup(buildAction, i, resultPath, false, dependentUpon, customTool);
							processed = true;
							write = true;
						}
					}

					if (processed)
						break;
				}

				if (!processed)
				{
					// we need to add a new ItemGroup after lastItemGroup
					// first find </ItemGroup>
					var insertPosition = 0;
					for (var i = lastItemGroupIndex; i < _contents.Count; i++)
					{
						if (_contents[i].Trim() == "</ItemGroup>")
						{
							insertPosition = i + 1;
							break;
						}
					}

					AddItemGroup(buildAction, insertPosition, resultPath, true, dependentUpon, customTool);
					write = true;
				}

				if (write)
					System.IO.File.WriteAllLines(_fileName, _contents);
			}
			else if (Type == ProjectType.DotNetCore)
			{
				if (dependentUpon == null && customTool == null && buildAction == BuildAction.Compile)
					return true;
			}

			return true;
		}

		void AddItemGroup(BuildAction buildAction, int insertPosition, string includeFile, bool createGroup, string dependentUponFile, string generator)
		{
			if (createGroup)
				_contents.Insert(insertPosition++, "  <ItemGroup>");
			_contents.Insert(insertPosition++, $"    <{buildAction} Include=\"{includeFile}\"" + (string.IsNullOrEmpty(dependentUponFile) && string.IsNullOrEmpty(generator) ? " />" : ">"));
			if (!string.IsNullOrEmpty(generator))
			{
				_contents.Insert(insertPosition++, $"      <Generator>{generator}</Generator>");
				_contents.Insert(insertPosition++, $"    </{buildAction}>");
			}

			if (!string.IsNullOrEmpty(dependentUponFile))
			{
				_contents.Insert(insertPosition++, $"      <DependentUpon>{dependentUponFile}</DependentUpon>");
				_contents.Insert(insertPosition++, $"    </{buildAction}>");
			}

			if (createGroup)
				_contents.Insert(insertPosition, "  </ItemGroup>");
		}

		protected string FindProperty(string propertyName)
		{
			foreach (var line in _contents)
			{
				if (line.Contains($"<{propertyName}>"))
					return line.Substring(line.IndexOf(">") + 1, line.IndexOf("<", line.IndexOf(">") + 1) - line.IndexOf(">") - 1);
			}

			return string.Empty;
		}

		internal string ProjectDirectory { get; set; }

		public static Project Load(string projectFileName)
		{
			var project = new Project {FileName = projectFileName};
			project.Load();
			return project;
		}

		internal static Project LoadFromFile(string fileName)
		{
			var project = new Project();
			try
			{
				var projectFileName = fileName.GetParentProject();
				return Load(projectFileName);
			}
			catch (Exception e)
			{
				project.Exception = e;
			}

			return project;
		}

		static Tuple<ProjectType, ProjectSubType> DetermineProjectType(string projectContents)
		{
			ProjectType type;
			ProjectSubType subType;

			if (projectContents.Contains(DotNetXmlNamespace))
			{
				type = ProjectType.DotNet;
				if (projectContents.Contains(VSProjectTypes.AspNetMvc5))
					subType = ProjectSubType.AspNet;
				else if (!projectContents.Contains("ProjectTypeGuids") && projectContents.Contains("<OutputType>Exe</OutputType>"))
					subType = ProjectSubType.Console;
				else if (!projectContents.Contains("ProjectTypeGuids") && projectContents.Contains("<OutputType>Library</OutputType>"))
					subType = ProjectSubType.Library;
				else if (projectContents.Contains(VSProjectTypes.Database))
					subType = ProjectSubType.Database;
				else if (projectContents.Contains(VSProjectTypes.PortableClassLibrary))
					subType = ProjectSubType.PortableClassLibrary;
				else if (projectContents.Contains(VSProjectTypes.XamarinAndroid))
					subType = ProjectSubType.XamarinAndroid;
				else if (projectContents.Contains(VSProjectTypes.XamarinIOS))
					subType = ProjectSubType.XamarinIOS;
				else if (projectContents.Contains(VSProjectTypes.XamarinMac))
					subType = ProjectSubType.XamarinMac;
				else if (projectContents.Contains(VSProjectTypes.XamarinTvOS))
					subType = ProjectSubType.XamarinTvOS;
				else
					subType = ProjectSubType.Unkown;
				return new Tuple<ProjectType, ProjectSubType>(type, subType);
			}

			if (projectContents.Contains("Project Sdk"))
			{
				type = ProjectType.DotNetCore;
				if (projectContents.Contains("Microsoft.NET.Sdk.Web"))
					subType = ProjectSubType.AspNet;
				else if (projectContents.Contains("Microsoft.NET.Sdk") && projectContents.Contains("<OutputType>Exe</OutputType>"))
					subType = ProjectSubType.Console;
				else if (projectContents.Contains("Microsoft.NET.Sdk") && !projectContents.Contains("<OutputType>Exe</OutputType>"))
					subType = ProjectSubType.Library;
				else
					subType = ProjectSubType.Unkown;
				return new Tuple<ProjectType, ProjectSubType>(type, subType);
			}


			throw new Exception("Project Type could not be determined.");
		}
	}
}
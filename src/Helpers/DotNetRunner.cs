using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Core.Helpers
{
	internal static class DotNetRunner
	{
		static DotNetRunner()
		{
			DotnetCommand = DotNetExe.FullPathOrDefault();
		}

		static string DotnetCommand { get; }

		internal static void Pack(string tempFolder)
		{
			ProcessRunner.Create(DotnetCommand)
				.WithArgument("pack")
				.WithArgument("-c", "release")
				.WithArgument("-o", tempFolder)
				.ReceiveOutput(s =>
				{
					Log.Debug(s);
					return true;
				})
				.Execute(true);
		}

		internal static int NugetPush(string nugetPackage, string nugetSource = null, string nugetApiKey = null)
		{
			var push = ProcessRunner.Create(DotnetCommand)
				.WithArgument("nuget")
				.WithArgument("push")
				.WithArgument(nugetPackage);

			if (!string.IsNullOrEmpty(nugetSource))
				push.WithArgument("--source", nugetSource);
			else
				push.WithArgument("--source", "https://api.nuget.org/v3/index.json");

			if (!string.IsNullOrEmpty(nugetApiKey)) push.WithArgument("--api-key", nugetApiKey);

			push.ReceiveOutput(s =>
			{
				Log.Debug(s);
				return true;
			});
			return push.Execute(true);
		}

		internal static void InstallOrUpdateLocal(string nupkgFile, string tempFolder)
		{
			var packageId = GetPackageId(nupkgFile);
			var version = GetPackageVersion(nupkgFile);

			if (DoesPackageExists(packageId))
				ProcessRunner.Create(DotnetCommand)
					.WithArgument("tool")
					.WithArgument("update")
					.WithArgument("-g")
					.WithArgument("--add-source", tempFolder)
					.WithArgument(packageId)
					.ReceiveOutput(s =>
					{
						Log.Debug(s);
						return true;
					})
					.Execute(true);
			else
				ProcessRunner.Create(DotnetCommand)
					.WithArgument("tool")
					.WithArgument("install")
					.WithArgument("-g")
					.WithArgument("--add-source", tempFolder)
					.WithArgument(packageId)
					.WithArgument("--version", version)
					.ReceiveOutput(s =>
					{
						Log.Debug(s);
						return true;
					})
					.Execute(true);
		}

		internal static bool Install(string packageId, string source, string version)
		{
			var install = ProcessRunner.Create(DotnetCommand)
				.WithArgument("tool")
				.WithArgument("install")
				.WithArgument("-g")
				.WithArgument(packageId)
				.ReceiveOutput(s =>
				{
					Log.Debug(s);
					return true;
				});

			if (!string.IsNullOrEmpty(source))
				install.WithArgument("--add-source", source);

			if (!string.IsNullOrEmpty(version))
				install.WithArgument("--version", version);

			var exitCode = install.Execute(true);
			return exitCode == 0;
		}

		internal static bool Update(string packageId, string source)
		{
			var update = ProcessRunner.Create(DotnetCommand)
				.WithArgument("tool")
				.WithArgument("update")
				.WithArgument("-g")
				.WithArgument(packageId)
				.ReceiveOutput(s =>
				{
					Log.Debug(s);
					return true;
				});

			if (!string.IsNullOrEmpty(source))
				update.WithArgument("--add-source", source);

			var exitCode = update.Execute(true);
			return exitCode == 0;
		}

		internal static bool UnInstall(string packageId)
		{
			var install = ProcessRunner.Create(DotnetCommand)
				.WithArgument("tool")
				.WithArgument("uninstall")
				.WithArgument("-g")
				.WithArgument(packageId)
				.ReceiveOutput(s =>
				{
					Log.Debug(s);
					return true;
				});
			var exitCode = install.Execute(true);
			return exitCode == 0;
		}

		internal static (bool success, string packageId, bool specificVersionInstalled) ResolvePackage(string packageId, string source, bool autoUpdate, string specificVersion)
		{
			var isFullPackageId = true;

			if (!packageId.ToLower().Contains(".genyman."))
			{
				isFullPackageId = false;
				packageId = ".genyman." + packageId;
			}

			var local = DoesPackageExists(packageId);
			var canContinue = false;
			var specificVersionInstalled = false;

			if (!local)
			{
				if (!isFullPackageId)
				{
					Log.Error($"Genyman package {packageId} is not installed. Auto-installation cannot be performed as {packageId} is not a fully qualified package Id.");
					{
						return (false, packageId, false);
					}
				}

				canContinue = Install(packageId, source, null);
			}
			else
			{
				// perform update, we need full package name
				var latest = GetLastestPackageVersion(packageId);
				packageId = latest.packageId; // always get full packageId here

				canContinue = latest.success;

				if (!string.IsNullOrEmpty(specificVersion) && latest.version != specificVersion)
				{
					// uninstall & install
					UnInstall(packageId);
					canContinue = Install(packageId, source, specificVersion);
					specificVersionInstalled = true;
				}
				else
				{
					if (canContinue && isFullPackageId && autoUpdate)
						Update(packageId, source);
				}
			}

			return (canContinue, packageId, specificVersionInstalled);
		}

		internal static string GetPackageId(string nupkgFile)
		{
			return string.Join(".", Path.GetFileNameWithoutExtension(nupkgFile).Split('.').Take(3));
		}

		internal static string GetPackageVersion(string nupkgFile)
		{
			return string.Join(".", Path.GetFileNameWithoutExtension(nupkgFile).Split('.').TakeLast(3));
		}

		internal static bool DoesPackageExists(string packageId)
		{
			var packageFolders = Directory.EnumerateDirectories(CliFolderPathCalculator.ToolsPackagePath);
			// check upon ending - if packageId is not complete
			var foundPackage = packageFolders.FirstOrDefault(f => f.ToLower().EndsWith(packageId.ToLower()));
			return foundPackage != null;
		}

		internal static (bool success, string packageId, string version) GetLastestPackageVersion(string packageId)
		{
			var packageFolders = Directory.EnumerateDirectories(CliFolderPathCalculator.ToolsPackagePath);
			var foundPackage = packageFolders.FirstOrDefault(f => f.ToLower().EndsWith(packageId.ToLower()));
			var subFolders = Directory.EnumerateDirectories(foundPackage, "*.*", SearchOption.TopDirectoryOnly);

			var foundPackageId = new DirectoryInfo(foundPackage).Name;
			var highestVersion = "0.0.0";
			var success = true;

			foreach (var subFolder in subFolders)
				try
				{
					var version = new DirectoryInfo(subFolder);
					if (new Version(version.Name) > new Version(highestVersion)) highestVersion = version.Name;
				}
				catch (Exception e)
				{
					Log.Debug(e.ToString());
					Log.Debug($"Could not parse version for {subFolder} folder");
				}

			if (highestVersion == "0.0.0") success = false;

			return (success, foundPackageId, highestVersion);
		}
		
		#region .NET CLI 
		
		// Copyright (c) .NET Foundation and contributors. All rights reserved.
		// Licensed under the MIT license. See LICENSE file in the project root for full license information.

		// https://github.com/dotnet/cli/blob/master/src/Microsoft.DotNet.Configurer/BashPathUnderHomeDirectory.cs
		internal struct BashPathUnderHomeDirectory
		{
			private readonly string _fullHomeDirectoryPath;
			private readonly string _pathRelativeToHome;

			public BashPathUnderHomeDirectory(string fullHomeDirectoryPath, string pathRelativeToHome)
			{
				_fullHomeDirectoryPath =
					fullHomeDirectoryPath ?? throw new ArgumentNullException(nameof(fullHomeDirectoryPath));
				_pathRelativeToHome = pathRelativeToHome ?? throw new ArgumentNullException(nameof(pathRelativeToHome));
			}

			public string PathWithTilde => $"~/{_pathRelativeToHome}";

			public string PathWithDollar => $"$HOME/{_pathRelativeToHome}";

			public string Path => $"{_fullHomeDirectoryPath}/{_pathRelativeToHome}";
		}
		
		// https://github.com/dotnet/cli/blob/master/src/Microsoft.DotNet.Configurer/CliFolderPathCalculator.cs
		internal static class CliFolderPathCalculator
		{
			public const string DotnetHomeVariableName = "DOTNET_CLI_HOME";
			private const string DotnetProfileDirectoryName = ".dotnet";
			private const string ToolsShimFolderName = "tools";

			public static string CliFallbackFolderPath =>
				Environment.GetEnvironmentVariable("DOTNET_CLI_TEST_FALLBACKFOLDER") ??
				Path.Combine(new DirectoryInfo(AppContext.BaseDirectory).Parent.FullName, "NuGetFallbackFolder");

			public static string ToolsShimPath => Path.Combine(DotnetUserProfileFolderPath, ToolsShimFolderName);

			public static string ToolsPackagePath => ToolPackageFolderPathCalculator.GetToolPackageFolderPath(ToolsShimPath);

			public static BashPathUnderHomeDirectory ToolsShimPathInUnix =>
				new BashPathUnderHomeDirectory(
					DotnetHomePath,
					Path.Combine(DotnetProfileDirectoryName, ToolsShimFolderName));

			public static string DotnetUserProfileFolderPath =>
				Path.Combine(DotnetHomePath, DotnetProfileDirectoryName);

			public static string PlatformHomeVariableName =>
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "USERPROFILE" : "HOME";

			public static string DotnetHomePath
			{
				get
				{
					var home = Environment.GetEnvironmentVariable(DotnetHomeVariableName);
					if (string.IsNullOrEmpty(home))
					{
						home = Environment.GetEnvironmentVariable(PlatformHomeVariableName);
						if (string.IsNullOrEmpty(home))
						{
							Log.Fatal($"Failed To Determine User Home Directory");
						}
					}

					return home;
				}
			}

		}
		
		// https://github.com/dotnet/cli/blob/master/src/Microsoft.DotNet.Configurer/ToolPackageFolderPathCalculator.cs
		internal static class ToolPackageFolderPathCalculator
		{
			private const string NestedToolPackageFolderName = ".store";
			public static string GetToolPackageFolderPath(string toolsShimPath)
			{
				return Path.Combine(toolsShimPath, NestedToolPackageFolderName);
			}
		}
		
		#endregion
	}
}
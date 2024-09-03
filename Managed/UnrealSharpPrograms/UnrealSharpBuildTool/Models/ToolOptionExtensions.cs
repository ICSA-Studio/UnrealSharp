using System.Reflection;
using System.Runtime.Versioning;

namespace UnrealSharpBuildTool.Models
{
    internal static class ToolOptionExtensions
    {
        public static FileInfo GetCSProjectFile(this ToolOptions options)
        {
            return new FileInfo(Path.Combine($"{options.ProjectDirectory}", $"{options.ProjectName}.csproj"));
        }
        public static FileInfo GetUProjectFilePath(this ToolOptions options)
        {
            return new FileInfo(Path.Combine(options.ProjectDirectory, $"{options.ProjectName}.uproject"));
        }
        public static string GetMSBuildConfig(this ToolOptions options)
        {
            return options.BuildConfig switch
            {
                BuildConfig.Debug => "Debug",
                BuildConfig.Release => "Release",
                BuildConfig.Publish => "Release",
                _ => throw new NotImplementedException()
            };
        }

        public static DirectoryInfo GetScriptFolder(this ToolOptions options)
        {
            return new DirectoryInfo(Path.Combine($"{options.ProjectDirectory}", "Script"));
        }

        public static DirectoryInfo GetScriptFolderBinaries(this ToolOptions options)
        {
            return new DirectoryInfo(Path.Combine($"{options.GetScriptFolder()}", "bin", options.GetMSBuildConfig(), options.GetVersion()));
        }

        public static string GetManagedProjectName(this ToolOptions options) => $"Managed{options.ProjectName}";

        public static DirectoryInfo OutputPath(this ToolOptions options)
        {
            var rootOutput = options.ArchiveDirectory ?? options.ProjectDirectory;

            if (options.BuildConfig == BuildConfig.Publish)
            {
                rootOutput = Path.Combine(rootOutput, options.ProjectName);
            }

            return new DirectoryInfo(Path.Combine(rootOutput, "Binaries", "Managed"));
        }

        public static DirectoryInfo GetManagedBinDir(this ToolOptions options)
        {
            return new DirectoryInfo(Path.Combine(options.PluginDirectory, "Binaries", "Managed"));
        }

        public static FileInfo GetWeaver(this ToolOptions options)
        {
            return new FileInfo(Path.Combine(options.GetManagedBinDir().FullName, "UnrealSharpWeaver.dll"));
        }

        public static string GetVersion(this ToolOptions options)
        {
            //TargetFrameworkAttribute str = (TargetFrameworkAttribute)options.GetType().Assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute)).First();
            var version = Environment.Version;
            return $"net{version.Major}.{version.Minor}";
        }
    }
}

using CommandLine;
using CommandLine.Text;

using UnrealSharpBuildTool.Actions;
using UnrealSharpBuildTool.Models;

namespace UnrealSharpBuildTool;

public static class Program
{
    internal static void SetSettings(ParserSettings settings)
    {
        settings.CaseInsensitiveEnumValues = true;
        settings.HelpWriter = null;
    }
    public static BuildToolOptions buildToolOptions;
    
    public static int Main(params string[] args)
    {
        using var parser = new Parser(SetSettings);
       
        var result = parser.ParseArguments<BuildToolOptions>(args);
        var results = parser.ParseArguments<ToolOptions>(args);
        results.WithParsed(result =>
        {
            Commands.Command value = result.Action switch
            {
                BuildAction.Build => new Commands.BuildSolution(result),
                BuildAction.Clean => new Commands.CleanSolution(result),
                BuildAction.Rebuild => new Commands.RebuildSolution(result),
                BuildAction.GenerateProject => new Commands.GenerateProject(result),
                BuildAction.Weave => new Commands.WeaveProject(result),
                BuildAction.Publish => new Commands.PublishProject(result),
                _ => throw new Exception($"Can't find build action with name \"{result.Action}\"")
            };
            value.Execute();
        });
        results.WithNotParsed(errors =>
        {
            var helpText = HelpText.AutoBuild(results, h =>
            {
                h.AutoHelp = false;     // hides --help
                h.AutoVersion = false;  // hides --version
                return HelpText.DefaultParsingErrorsHandler(results, h);
            }, e => e);
            Console.WriteLine(helpText);
        });
        try
        {
            //Parser parser = new(with => with.HelpWriter = null);
            //var result = parser.ParseArguments<BuildToolOptions>(args);
            
            if (result.Tag == ParserResultType.NotParsed)
            {
                BuildToolOptions.PrintHelp(result);
                throw new Exception("Invalid arguments.");
            }
        
            buildToolOptions = result.Value;
            
            if (!BuildToolAction.InitializeAction())
            {
                throw new Exception("Failed to initialize action.");
            }
            
            Console.WriteLine($"UnrealSharpBuildTool executed {buildToolOptions.Action} action successfully.");
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
        
        return 0;
    }

    public static string GetCSProjectFile()
    {
        return buildToolOptions.ProjectName + ".sln";
    }

    public static string GetUProjectFilePath()
    {
        return Path.Combine(buildToolOptions.ProjectDirectory, buildToolOptions.ProjectName + ".uproject");
    }
    
    public static string GetScriptFolderBinaries()
    {
        string currentBuildConfig = GetBuildConfiguration(buildToolOptions.BuildConfig);
        return Path.Combine(GetScriptFolder(), "bin", currentBuildConfig, GetVersion());
    }
    
    public static string GetBuildConfiguration(BuildConfig buildConfig)
    {
        return buildConfig switch
        {
            BuildConfig.Debug => "Debug",
            BuildConfig.Release => "Release",
            BuildConfig.Publish => "Release",
            _ => "Release"
        };
    }
    
    public static string GetScriptFolder()
    {
        return Path.Combine(buildToolOptions.ProjectDirectory, "Script");
    }
    
    public static string GetProjectDirectory()
    {
        return buildToolOptions.ProjectDirectory;
    }


    public static string ManagedProjectName => $"Managed{buildToolOptions.ProjectName}";
    
    public static string OutputPath()
    {
        string rootOutput = buildToolOptions.ArchiveDirectory ?? buildToolOptions.ProjectDirectory;
        
        if (buildToolOptions.BuildConfig == BuildConfig.Publish)
        {
            rootOutput = Path.Combine(rootOutput, buildToolOptions.ProjectName);
        }
        
        return Path.Combine(rootOutput, "Binaries", "Managed");
    }

    public static string GetWeaver()
    {
        return Path.Combine(GetManagedBinariesDirectory(), "UnrealSharpWeaver.dll");
    }

    public static string GetManagedBinariesDirectory()
    {
        return Path.Combine(buildToolOptions.PluginDirectory, "Binaries", "Managed");
    }

    public static string GetVersion()
    {
        Version currentVersion = Environment.Version;
        string currentVersionStr = $"{currentVersion.Major}.{currentVersion.Minor}";
        return "net" + currentVersionStr;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace UnrealSharpBuildTool.Models
{
    public record ToolOptions
    {
        [Option(Required = true, HelpText = "The action the build tool should process. Build / Clean / GenerateProjects")]
        public BuildAction Action { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        [Option(Required = true, HelpText = "The directory where the .uproject file resides.")]
        public string ProjectDirectory { get; set; }

        [Option(Required = true, HelpText = "The name of the Unreal Engine project.")]
        public string ProjectName { get; set; }

        [Option(HelpText = "The path to the dotnet.exe")]
        public string DotNetExecutable { get; set; }

        [Option(HelpText = "Build with debug or release")]
        public BuildConfig BuildConfig { get; set; }

        [Option(HelpText = "The UnrealSharp plugin directory.")]
        public string PluginDirectory { get; set; }

        [Option(HelpText = "The Unreal Engine directory.")]
        public string EngineDirectory { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        [Option(HelpText = "The directory where the archive should be stored.")]
        public string? ArchiveDirectory { get; set; }

        //public static void PrintHelp(ParserResult<BuildToolOptions> result)
        //{
            
        //    string name = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
        //    //CommandLine.Text.HelpText.AutoBuild


        //    //CommandLine.Parser.Default.Settings.
        //    var helpText = HelpText.AutoBuild(result, h => h, e => e);
        //    Console.WriteLine(helpText);
        //}
    }
}

using UnrealSharpBuildTool.Models;

namespace UnrealSharpBuildTool.Commands;

public class WeaveProject(ToolOptions options) : Command(options)
{
    public override string[] BuildArguments(ToolOptions options)
    {
        var weaver = options.GetWeaver().ReturnIfExists();

        var scriptFolderBinaries = options.GetScriptFolderBinaries();
        var outputPath = options.OutputPath();
        var projectName = options.GetManagedProjectName();

        return [
            weaver.FullName,
            "-p",
            outputPath.FullName,
            "-o",
            scriptFolderBinaries.FullName,
            "-n",
            projectName
        ];
    }
}
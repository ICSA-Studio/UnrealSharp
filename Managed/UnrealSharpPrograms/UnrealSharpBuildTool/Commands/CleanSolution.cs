using UnrealSharpBuildTool.Models;

namespace UnrealSharpBuildTool.Commands;

public class CleanSolution(ToolOptions options) : Command(options)
{

    internal override string[] BuildArguments(ToolOptions options)
    {
        var outPath = options.OutputPath();
        var slnDir = options.GetScriptFolder();
        if (!slnDir.Exists)
        {
            throw new DirectoryNotFoundException(slnDir.FullName);
        }
        if (outPath.Exists)
        {
            outPath.Delete(true);
        }
        return [
            "clean",
            slnDir.FullName
        ];
    }
}
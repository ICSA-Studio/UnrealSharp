using UnrealSharpBuildTool.Models;

namespace UnrealSharpBuildTool.Commands;

internal class BuildSolution : Command
{

    internal BuildSolution(ToolOptions options) : base(options)
    {}

    internal override string[] BuildArguments(ToolOptions options)
    {
        var slnDir = options.GetScriptFolder().ReturnIfExists();
        return
        [
            options.BuildConfig == BuildConfig.Publish ? "publish" : "build",
            slnDir.FullName,
            "--configuration",
            options.GetMSBuildConfig()
        ];
    }
}
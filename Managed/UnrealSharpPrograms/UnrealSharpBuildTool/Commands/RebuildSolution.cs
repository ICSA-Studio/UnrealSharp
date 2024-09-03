using UnrealSharpBuildTool.Models;

namespace UnrealSharpBuildTool.Commands;

public class RebuildSolution(ToolOptions options) : Command(options)
{
    public override void BeforeExecute()
    {
        CleanSolution cleanSolutionProcess = new(Options);

        cleanSolutionProcess.Execute();

        BuildSolution buildSolution = new(Options);

        buildSolution.Execute();
    }

    public override string[] BuildArguments(ToolOptions options)
    {
        return [];
    }
}
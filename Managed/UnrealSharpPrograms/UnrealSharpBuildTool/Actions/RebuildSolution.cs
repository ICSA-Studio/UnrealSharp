namespace UnrealSharpBuildTool.Actions;

public class RebuildSolution : BuildSolution
{
    public override bool RunAction()
    {
        CleanSolution cleanSolutionProcess = new();
        
        if (!cleanSolutionProcess.RunAction())
        {
            return false;
        }

        BuildSolution buildSolution = new();
        
        if (!buildSolution.RunAction())
        {
            return false;
        }

        return true;
    }
}
using System.Collections.ObjectModel;

using UnrealSharpBuildTool.Models;

namespace UnrealSharpBuildTool.Commands;

public class PublishProject(ToolOptions options) : Command(options)
{
    public override void BeforeExecute()
    {
        var build = new BuildSolution(Options);

        build.Execute(
            "--self-contained",
            "--runtime",
            "win-x64",
            $"-p:PublishDir=\"{Options.OutputPath().FullName}\""
        );
        var weave = new WeaveProject(Options);
        weave.Execute();
    }

    public override bool RunAction()
    {
        // Force the build configuration to be Publish, for now.
        // I'm gonna rewrite this later anyways.
        Program.buildToolOptions.BuildConfig = BuildConfig.Publish;

        string bindingsPath = Path.Combine(Program.buildToolOptions.PluginDirectory, "Managed", "UnrealSharp");

        Collection<string> extraArguments =
        [
            "--self-contained",
            "--runtime",
            "win-x64",
            $"-p:PublishDir=\"{Program.GetOutputPath()}\""
        ];

        BuildSolution.StartBuildingSolution(bindingsPath, Program.buildToolOptions.BuildConfig, extraArguments);

        BuildSolution buildSolution = new();
        buildSolution.RunAction();

        WeaveProject weaveProject = new();
        weaveProject.RunAction();

        return true;
    }

    public override string[] BuildArguments(ToolOptions options)
    {
        // Force the build configuration to be Publish, for now.
        // I'm gonna rewrite this later anyways.
        Program.buildToolOptions.BuildConfig = BuildConfig.Publish;

        string bindingsPath = Path.Combine(options.PluginDirectory, "Managed", "UnrealSharp");

        return [

        ];
    }
}
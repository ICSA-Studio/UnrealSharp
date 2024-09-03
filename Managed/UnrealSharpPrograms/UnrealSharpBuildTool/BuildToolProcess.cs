using System.Diagnostics;
using System.Collections.Generic;

namespace UnrealSharpBuildTool;

public class BuildToolProcess : Process
{
    public BuildToolProcess(string? fileName = null)
    {
        FileInfo info = new(fileName ?? "");
        
        fileName ??= Program.buildToolOptions.DotNetExecutable ?? "dotnet";
        StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = string.Join(' ', Program.GetCSProjectFile(), "build", "--configuration", Program.GetBuildConfiguration(Program.buildToolOptions.BuildConfig))
        };
    }

    public bool StartBuildToolProcess()
    {
        try
        {
            if (!Start())
            {
                throw new Exception("Failed to start process");
            }
            
            string output = StandardOutput.ReadToEnd();
            string error = StandardError.ReadToEnd();
            
            WaitForExit();

            if (ExitCode != 0)
            {
                throw new Exception($"Error in executing build command {StartInfo.Arguments}: {Environment.NewLine + error + Environment.NewLine + output}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }

        return true;
    }
}
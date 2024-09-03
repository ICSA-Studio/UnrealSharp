using Microsoft.Build.CommandLine;
using Microsoft.Build.Tasks;

using System.Diagnostics;
using UnrealSharpBuildTool.Models;

namespace UnrealSharpBuildTool.Commands
{
    public abstract class Command<TOptions> where TOptions : notnull
    {
        public TOptions Options { get; }
        public ProcessStartInfo StartInfo { get; private set; }
        public List<string> Args { get; }
        private readonly List<Action<TOptions>> steps = [];
        public Command(TOptions options)
        {
            Options = options;
            Args = [];
            StartInfo = new ProcessStartInfo
            {
                FileName = options.DotNetExecutable ?? (OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet"),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                /// This is toggled by default because this app is a .NET Core app.
                //UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        public Command<TOptions> WithStep(Action<TOptions> step)
        {
            steps.Add(step);
            return this;
        }
        public Command<TOptions> WithSteps(params Action<TOptions>[] steps)
        {
            this.steps.AddRange(steps);
            return this;
        }
        public Command<TOptions> WithStartInfo(Func<TOptions, ProcessStartInfo> startInfo) {
            StartInfo = startInfo(Options);
            return this;
        }

        //public virtual void BeforeExecute()
        //{}

        //public virtual void AfterExecute()
        //{}

        public Command<TOptions> WithArgs(Func<TOptions, string[]> argBuilder)
        {
            Args.AddRange(argBuilder(Options));
            return this;
        }
        public abstract string[] BuildArguments(ToolOptions options);

        public void Execute()
        {
            //var args = BuildArguments(Options).ToList();
            //args.AddRange(extraArgs);
            StartInfo.Arguments = string.Join(' ', Args.Where(arg => !string.IsNullOrWhiteSpace(arg)));
            //BeforeExecute();

            using var process = Process.Start(StartInfo);
            if (process is not null)
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Console.Error.WriteLine($"Error in executing build command {StartInfo.Arguments}");
                }
            }
            //AfterExecute();
        }

        public async Task ExecuteAsync()
        {
            StartInfo.Arguments = string.Join(' ', BuildArguments(Options).Where(arg => !string.IsNullOrWhiteSpace(arg)));

            using var process = Process.Start(StartInfo);
            if (process is not null)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode != 0)
                {
                    Console.Error.WriteLine($"Error in executing build command {StartInfo.Arguments}");
                }
            }
        }
    }
}

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace UnrealSharpBuildTool.Models
{
    public enum CommandName
    {
        Executable,
        Project,
        IIS,
        IISExpress,
        DebugRoslynComponent,
        Docker,
        DockerCompose,
        MsixPackage,
        SdkContainer,
        WSL2
    }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public class LaunchProfiles
    {
        [JsonPropertyName("profiles")] public ProfilesObject Profiles { get; set; }
    }
    
    public class ProfilesObject : Dictionary<string, Profile>
    {
        [JsonPropertyName("UnrealSharp")] public Profile ProfileName { get; set; }
    }
    
    public class Profile
    {
        [JsonPropertyName("commandName")] public CommandName CommandName { get; set; }
        [JsonPropertyName("executablePath")] public string ExecutablePath { get; set; }
        [JsonPropertyName("commandLineArgs")] public string CommandLineArgs { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}

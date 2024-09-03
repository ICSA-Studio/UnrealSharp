using System.Runtime.InteropServices;
using System.Xml;
using Newtonsoft.Json;

using UnrealSharpBuildTool.Models;

namespace UnrealSharpBuildTool.Commands;

public class GenerateProject(ToolOptions options) : Command(options)
{
    public override bool RunAction()
    {
        string projectName = Program.ManagedProjectName;
        string folder = Program.GetScriptFolder();
        string csProjName = $"{projectName}.csproj";

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        string csProjPath = Path.Combine(folder, csProjName);
        if (!File.Exists(csProjPath))
        {
            string version = Program.GetVersion();
            BuildToolProcess generateProjectProcess = new();
            Path.Join
            // Create a class library.
            generateProjectProcess.StartInfo.ArgumentList.Add("new");
            generateProjectProcess.StartInfo.ArgumentList.Add("classlib");
        
            // Assign project name to the class library.
            generateProjectProcess.StartInfo.ArgumentList.Add("-n");
            generateProjectProcess.StartInfo.ArgumentList.Add(projectName);
        
            // Set the output directory to the current directory.
            generateProjectProcess.StartInfo.ArgumentList.Add("-o");
            generateProjectProcess.StartInfo.ArgumentList.Add(".");
        
            // Set the target framework to the current version.
            generateProjectProcess.StartInfo.ArgumentList.Add("-f");
            generateProjectProcess.StartInfo.ArgumentList.Add(version);
        
            generateProjectProcess.StartInfo.WorkingDirectory = folder;

            if (!generateProjectProcess.StartBuildToolProcess())
            {
                return false;
            }
            
            // dotnet new class lib generates a file named Class1, remove it.
            string myClassFile = Path.Combine(folder, "Class1.cs");
            if (File.Exists(myClassFile))
            {
                File.Delete(myClassFile);
            }
            AddLaunchSettings();
        }
        
        ModifyCSProjFile(folder, csProjName);
        
        if (!File.Exists(Path.Combine(folder, $"{projectName}.sln")))
        {
            BuildToolProcess generateSln = new();
        
            // Create a solution.
            generateSln.StartInfo.ArgumentList.Add("new");
            generateSln.StartInfo.ArgumentList.Add("sln");
        
            // Assign project name to the solution.
            generateSln.StartInfo.ArgumentList.Add("-n");
            generateSln.StartInfo.ArgumentList.Add(projectName);
            generateSln.StartInfo.WorkingDirectory = folder;
        
            // Force the creation of the solution.
            generateSln.StartInfo.ArgumentList.Add("--force");
        
            if (!generateSln.StartBuildToolProcess())
            {
                return false;
            }
        
            BuildToolProcess addProjectToSln = new();
        
            // Add the project to the solution.
            addProjectToSln.StartInfo.ArgumentList.Add("sln");
            addProjectToSln.StartInfo.ArgumentList.Add("add");
            addProjectToSln.StartInfo.ArgumentList.Add(csProjName);
        
            addProjectToSln.StartInfo.WorkingDirectory = folder;

            if (!addProjectToSln.StartBuildToolProcess())
            {
                return false;
            }
        }
        
        BuildSolution buildSolution = new();
        if (!buildSolution.RunAction())
        {
            return false;
        }

        WeaveProject weaveProject = new();
        if (!weaveProject.RunAction())
        {
            return false;
        }

        return true;
    }

    private void ModifyCSProjFile(string folder, string csProjName)
    {
        string csProjPath = Path.Combine(folder, csProjName);

        try
        {
            XmlDocument csprojDocument = new();
            csprojDocument.Load(csProjPath);

            if (csprojDocument.SelectSingleNode("//ItemGroup") is not XmlElement newItemGroup)
            {
                newItemGroup = csprojDocument.CreateElement("ItemGroup");
                _ = csprojDocument.DocumentElement?.AppendChild(newItemGroup);
            }
            
            AppendProperties(csprojDocument);
            AppendUnrealSharpReference(csprojDocument, newItemGroup);
            AppendSourceGeneratorReference(csprojDocument, newItemGroup);
            AppendGeneratedCode(csprojDocument, newItemGroup);
            
            csprojDocument.Save(csProjPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while updating the .csproj file: {ex.Message}", ex);
        }
    }
    
    void AddProperty(string name, string value, XmlDocument doc, XmlNode propertyGroup)
    {
        XmlNode? newProperty = propertyGroup.SelectSingleNode(name);
        
        if (newProperty == null)
        {
            newProperty = doc.CreateElement(name);
            propertyGroup.AppendChild(newProperty);
        }
        
        newProperty.InnerText = value; 
    }

    private void AppendProperties(XmlDocument doc)
    {
        XmlNode? propertyGroup = doc.SelectSingleNode("//PropertyGroup") ?? doc.CreateElement("PropertyGroup");
        AddProperty("CopyLocalLockFileAssembliesName", "true", doc, propertyGroup);
        AddProperty("AllowUnsafeBlocks", "true", doc, propertyGroup);
        AddProperty("EnableDynamicLoading", "true", doc, propertyGroup);
    }

    string GetPathToBinaries()
    {
        string unrealSharpPath = GetUnrealSharpPathRelativeToPlugins();
        return Path.Combine(unrealSharpPath, "Binaries", "Managed");
    }
    
    private bool ElementExists(XmlDocument doc, string elementName, string attributeName, string attributeValue)
    {
        XmlNodeList nodes = doc.GetElementsByTagName(elementName);
        foreach (XmlNode node in nodes)
        {
            if (node.Attributes?[attributeName]?.Value == attributeValue)
            {
                return true;
            }
            else if (!string.IsNullOrEmpty(attributeValue) && node.Attributes?[attributeName]?.Value.Contains(attributeValue) == true)
            {
                return true;
            }
        }
        return false;
    }

    private void AppendUnrealSharpReference(XmlDocument doc, XmlElement itemGroup)
    {
        if (ElementExists(doc, "Reference", "Include", "UnrealSharp"))
        {
            return;
        }
        
        XmlElement unrealSharpReference = doc.CreateElement("Reference");
        unrealSharpReference.SetAttribute("Include", "UnrealSharp");

        XmlElement newHintPath = doc.CreateElement("HintPath");
        newHintPath.InnerText = Path.Combine(GetPathToBinaries(), Program.GetVersion(), "UnrealSharp.dll");
        unrealSharpReference.AppendChild(newHintPath);
        itemGroup.AppendChild(unrealSharpReference);
    }
    
    private void AppendSourceGeneratorReference(XmlDocument doc, XmlElement itemGroup)
    {
        string sourceGeneratorPath = Path.Combine(GetPathToBinaries(), "UnrealSharp.SourceGenerators.dll");
        if (ElementExists(doc, "Analyzer", "Include", sourceGeneratorPath))
        {
            return;
        }
        
        XmlElement sourceGeneratorReference = doc.CreateElement("Analyzer");
        sourceGeneratorReference.SetAttribute("Include", sourceGeneratorPath);
        itemGroup.AppendChild(sourceGeneratorReference);
    }
    
    private void AppendGeneratedCode(XmlDocument doc, XmlElement itemGroup)
    {
        string attributeValue = "obj/generated/**/*.cs";
        if (ElementExists(doc, "Compile", "Include", attributeValue))
        {
            return;
        }
        
        XmlElement generatedCode = doc.CreateElement("Compile");
        generatedCode.SetAttribute("Include", attributeValue);
        itemGroup.AppendChild(generatedCode);
    }
    
    private string GetUnrealSharpPathRelativeToPlugins()
    {
        string basePath = Program.buildToolOptions.ProjectDirectory;
        string targetPath = Path.Combine(basePath, Program.buildToolOptions.PluginDirectory);
        
        Uri baseUri = new(basePath + (OperatingSystem.IsWindows() ? @"\" : "/"));
        Uri targetUri = new(targetPath);
        
        Uri relativeUri = baseUri.MakeRelativeUri(targetUri);
        
        string relativePath = OperatingSystem.IsWindows() ? Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', '\\') : Uri.UnescapeDataString(relativeUri.ToString());

        return relativePath;
    }
    
    void AddLaunchSettings()
    {
        string propertiesDirectoryPath = Path.Combine(Program.GetScriptFolder(), "Properties");
        string launchSettingsPath = Path.Combine(propertiesDirectoryPath, "launchSettings.json");

        if (!Directory.Exists(propertiesDirectoryPath))
        {
            Directory.CreateDirectory(propertiesDirectoryPath);
        }
        
        if (File.Exists(launchSettingsPath))
        {
            return;
        }
        
        CreateOrUpdateLaunchSettings(launchSettingsPath);
    }
    
    void CreateOrUpdateLaunchSettings(string launchSettingsPath)
    {
        LaunchProfiles root = new();

        string executable = OperatingSystem.IsWindows() ? "UnrealEditor.exe" : "UnrealEditor";

        string os = "Unknown";
        if (OperatingSystem.IsWindows())
        {
            os = "Win64";
        }
        else if (OperatingSystem.IsMacOS())
        {
            os = "Mac";
        }
        else if (OperatingSystem.IsLinux())
        {
            os = "Linux";
        }
        else if (OperatingSystem.IsTvOS())
        {
            os = "TVOS";
        }
        else if (OperatingSystem.IsIOS())
        {
            os = "IOS";
        }
        else if (OperatingSystem.IsAndroid())
        {
            os = "Android";
        }
        
        string commandLineArgs = Options.GetUProjectFilePath().FullName;
        var executablePath = new FileInfo(Path.Combine(Options.GetScriptFolderBinaries().ReturnIfExists().FullName, os, executable));
        // Create a new profile if it doesn't exist
        if (root.Profiles == null)
        {
            root.Profiles = new Profiles();
        }
            
        root.Profiles.ProfileName = new Profile
        {
            CommandName = "Executable",
            ExecutablePath = ,
            CommandLineArgs = $"\"{commandLineArgs}\"",
        };
        
        string newJsonString = JsonConvert.SerializeObject(root, Newtonsoft.Json.Formatting.Indented);
        StreamWriter writer = File.CreateText(launchSettingsPath);
        writer.Write(newJsonString);
        writer.Close();
    }

    public override void BeforeExecute()
    {
        var scripts = Options.GetScriptFolder();
        scripts.CreateIfNotExists();
        base.BeforeExecute();
    }

    

    public string[] BuildArguments()
    {
        var scripts = Options.GetScriptFolder();
        return [
            "new",
            "classlib",
            "-n",
            Options.GetManagedProjectName(),
            "-o",
            scripts.FullName,
            "-f",
            Options.GetVersion()

        ];
    }
}

public record LaunchProfiles
{
    [JsonProperty("profiles")]
    public Profiles Profiles { get; set; }
}
public record Profiles
{
    [JsonProperty("UnrealSharp")]
    public Profile ProfileName { get; set; }
}

public record Profile
{
    [JsonProperty("commandName")]
    public string CommandName { get; set; }

    [JsonProperty("executablePath")]
    public string ExecutablePath { get; set; }

    [JsonProperty("commandLineArgs")]
    public string CommandLineArgs { get; set; }
}
namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public sealed class ConfigTemplateModel
{
    public string Name { get; }
    public string FilePath { get; }
    public string Version { get; }

    public ConfigTemplateModel(string name, string filePath, string version)
    {
        Name = name;
        FilePath = filePath;
        Version = version;
    }
}

namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public sealed class ConfigTemplateManifest
{
    public List<ConfigTemplateManifestEntry> Entries { get; set; } = new();
}

public sealed class ConfigTemplateManifestEntry
{
    public string Name { get; set; } = "";
    public string File { get; set; } = "";
    public string Version { get; set; } = "";
}

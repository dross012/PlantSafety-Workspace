namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public sealed class ConfigTemplateOptions
{
    public string TemplateDirectory { get; }
    public string ManifestFileName { get; }

    public ConfigTemplateOptions(string templateDirectory, string manifestFileName = "configtemplates.manifest.json")
    {
        TemplateDirectory = templateDirectory;
        ManifestFileName = manifestFileName;
    }

    public string ManifestPath => Path.Combine(TemplateDirectory, ManifestFileName);
}

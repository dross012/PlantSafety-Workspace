using System.Text.Json;

namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public sealed class ConfigTemplateManifestLoader
{
    private readonly ConfigTemplateOptions _options;

    public ConfigTemplateManifestLoader(ConfigTemplateOptions options)
    {
        _options = options;
    }

    public ConfigTemplateManifest Load()
    {
        var path = _options.ManifestPath;

        if (!File.Exists(path))
            throw new FileNotFoundException($"ConfigTemplate manifest not found: {path}");

        var json = File.ReadAllText(path);

        var manifest = JsonSerializer.Deserialize<ConfigTemplateManifest>(json)
                       ?? throw new InvalidOperationException("Invalid ConfigTemplate manifest JSON.");

        return manifest;
    }
}

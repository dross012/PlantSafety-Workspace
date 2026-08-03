using System.Text.Json;

namespace PlantClientInstaller.Core.Payload.Assets;

public sealed class AssetManifestLoader
{
    private readonly AssetOptions _options;

    public AssetManifestLoader(AssetOptions options)
    {
        _options = options;
    }

    public AssetManifest Load()
    {
        var path = _options.ManifestPath;

        if (!File.Exists(path))
            throw new FileNotFoundException($"Asset manifest not found: {path}");

        var json = File.ReadAllText(path);

        var dto = JsonSerializer.Deserialize<List<AssetManifestEntry>>(json)
                  ?? throw new InvalidOperationException("Invalid asset manifest JSON.");

        return new AssetManifest(dto);
    }
}

using System.Text.Json;

namespace PlantClientInstaller.Core.Payload.ClientApp;

public sealed class ClientAppManifestLoader
{
    private readonly ClientAppOptions _options;

    public ClientAppManifestLoader(ClientAppOptions options)
    {
        _options = options;
    }

    public ClientAppManifest Load()
    {
        var path = _options.ManifestPath;

        if (!File.Exists(path))
            throw new FileNotFoundException($"ClientApp manifest not found: {path}");

        var json = File.ReadAllText(path);

        var manifest = JsonSerializer.Deserialize<ClientAppManifest>(json)
                       ?? throw new InvalidOperationException("Invalid ClientApp manifest JSON.");

        return manifest;
    }
}

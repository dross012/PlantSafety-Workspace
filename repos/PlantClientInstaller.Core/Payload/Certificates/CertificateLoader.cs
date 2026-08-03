using System.Text.Json;

namespace PlantClientInstaller.Core.Payload.Certificates;

public sealed class CertificateLoader
{
    private readonly CertificateOptions _options;

    public CertificateLoader(CertificateOptions options)
    {
        _options = options;
    }

    public IReadOnlyList<CertificateManifestEntry> LoadManifest()
    {
        var path = _options.ManifestPath;

        if (!File.Exists(path))
            throw new FileNotFoundException($"Certificate manifest not found: {path}");

        var json = File.ReadAllText(path);

        var dto = JsonSerializer.Deserialize<List<CertificateManifestEntry>>(json)
                  ?? throw new InvalidOperationException("Invalid certificate manifest JSON.");

        return dto;
    }
}

public sealed class CertificateManifestEntry
{
    public string Name { get; set; } = "";
    public string File { get; set; } = "";
}

namespace PlantClientInstaller.Core.Payload.Assets;

public sealed class AssetManifest
{
    public IReadOnlyList<AssetManifestEntry> Entries { get; }

    public AssetManifest(IReadOnlyList<AssetManifestEntry> entries)
    {
        Entries = entries;
    }
}

public sealed class AssetManifestEntry
{
    public string Name { get; set; } = "";
    public string File { get; set; } = "";
    public string ChecksumAlgorithm { get; set; } = "";
    public string ChecksumValue { get; set; } = "";
}

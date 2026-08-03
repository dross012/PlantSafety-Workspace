namespace PlantClientInstaller.Core.Payload.Assets;

public sealed class AssetOptions
{
    public string AssetDirectory { get; }
    public string ManifestFileName { get; }

    public AssetOptions(string assetDirectory, string manifestFileName = "assets.manifest.json")
    {
        AssetDirectory = assetDirectory;
        ManifestFileName = manifestFileName;
    }

    public string ManifestPath => Path.Combine(AssetDirectory, ManifestFileName);
}

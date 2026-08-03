namespace PlantClientInstaller.Core.Payload.Assets;

public static class AssetExtensions
{
    public static bool IsExecutable(this AssetModel model) =>
        model.FilePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);

    public static bool IsLibrary(this AssetModel model) =>
        model.FilePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);

    public static bool IsManifest(this AssetModel model) =>
        model.FilePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
}

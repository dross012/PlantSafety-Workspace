namespace PlantClientInstaller.Core.Payload.Assets;

public sealed class AssetModel
{
    public string Name { get; }
    public string FilePath { get; }
    public long SizeBytes { get; }
    public AssetChecksum Checksum { get; }

    public AssetModel(string name, string filePath, long sizeBytes, AssetChecksum checksum)
    {
        Name = name;
        FilePath = filePath;
        SizeBytes = sizeBytes;
        Checksum = checksum;
    }
}

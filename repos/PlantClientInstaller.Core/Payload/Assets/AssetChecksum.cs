namespace PlantClientInstaller.Core.Payload.Assets;

public sealed class AssetChecksum
{
    public string Algorithm { get; }
    public string Value { get; }

    public AssetChecksum(string algorithm, string value)
    {
        Algorithm = algorithm;
        Value = value;
    }
}

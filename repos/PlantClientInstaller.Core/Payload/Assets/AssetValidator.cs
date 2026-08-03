using System.Security.Cryptography;

namespace PlantClientInstaller.Core.Payload.Assets;

public sealed class AssetValidator
{
    public void ValidateManifest(AssetManifest manifest, AssetPolicy policy)
    {
        if (!policy.RequireManifestIntegrity)
            return;

        if (manifest.Entries.Count == 0)
            throw new InvalidOperationException("Asset manifest contains no entries.");

        foreach (var entry in manifest.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
                throw new InvalidOperationException("Asset entry missing Name.");

            if (string.IsNullOrWhiteSpace(entry.File))
                throw new InvalidOperationException($"Asset '{entry.Name}' missing File path.");

            if (string.IsNullOrWhiteSpace(entry.ChecksumAlgorithm))
                throw new InvalidOperationException($"Asset '{entry.Name}' missing checksum algorithm.");

            if (string.IsNullOrWhiteSpace(entry.ChecksumValue))
                throw new InvalidOperationException($"Asset '{entry.Name}' missing checksum value.");
        }
    }

    public void ValidateChecksum(string filePath, AssetChecksum checksum, AssetPolicy policy)
    {
        if (!policy.RequireChecksumValidation)
            return;

        using var stream = File.OpenRead(filePath);

        string computed = checksum.Algorithm.ToUpperInvariant() switch
        {
            "SHA256" => Convert.ToHexString(SHA256.HashData(stream)),
            "SHA1" => Convert.ToHexString(SHA1.HashData(stream)),
            _ => throw new NotSupportedException($"Unsupported checksum algorithm: {checksum.Algorithm}")
        };

        if (!computed.Equals(checksum.Value, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Checksum mismatch for asset '{filePath}'.");
    }
}

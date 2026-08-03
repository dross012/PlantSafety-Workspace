namespace PlantClientInstaller.Core.Payload.Assets;

public sealed class AssetPolicy
{
    public bool RequireChecksumValidation { get; }
    public bool RequireManifestIntegrity { get; }

    public AssetPolicy(
        bool requireChecksumValidation = true,
        bool requireManifestIntegrity = true)
    {
        RequireChecksumValidation = requireChecksumValidation;
        RequireManifestIntegrity = requireManifestIntegrity;
    }
}

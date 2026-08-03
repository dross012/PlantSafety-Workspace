namespace PlantClientInstaller.Core.Payload.Certificates;

public sealed class CertificatePolicy
{
    public bool RequireValidDateRange { get; }
    public bool RequireTrustedRoot { get; }
    public bool RequireEnhancedKeyUsage { get; }
    public string? RequiredEkuOid { get; }

    public CertificatePolicy(
        bool requireValidDateRange = true,
        bool requireTrustedRoot = true,
        bool requireEnhancedKeyUsage = false,
        string? requiredEkuOid = null)
    {
        RequireValidDateRange = requireValidDateRange;
        RequireTrustedRoot = requireTrustedRoot;
        RequireEnhancedKeyUsage = requireEnhancedKeyUsage;
        RequiredEkuOid = requiredEkuOid;
    }
}

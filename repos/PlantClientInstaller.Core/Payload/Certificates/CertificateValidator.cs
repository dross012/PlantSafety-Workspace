using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PlantClientInstaller.Core.Payload.Certificates;

public sealed class CertificateValidator
{
    public void Validate(X509Certificate2 cert, CertificatePolicy policy)
    {
        if (policy.RequireValidDateRange)
        {
            if (DateTime.UtcNow < cert.NotBefore || DateTime.UtcNow > cert.NotAfter)
                throw new InvalidOperationException($"Certificate '{cert.Subject}' is expired or not yet valid.");
        }

        if (policy.RequireTrustedRoot)
        {
            using var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            if (!chain.Build(cert))
                throw new InvalidOperationException($"Certificate '{cert.Subject}' is not trusted.");
        }

        if (policy.RequireEnhancedKeyUsage && policy.RequiredEkuOid is not null)
        {
            var eku = cert.Extensions.OfType<X509EnhancedKeyUsageExtension>().FirstOrDefault();
            if (eku is null || !eku.EnhancedKeyUsages.Cast<Oid>().Any(o => o.Value == policy.RequiredEkuOid))
                throw new InvalidOperationException($"Certificate '{cert.Subject}' missing required EKU: {policy.RequiredEkuOid}");
        }
    }
}

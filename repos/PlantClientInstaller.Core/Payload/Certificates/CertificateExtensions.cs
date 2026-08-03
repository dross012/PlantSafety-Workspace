namespace PlantClientInstaller.Core.Payload.Certificates;

public static class CertificateExtensions
{
    public static bool IsCodeSigningCert(this CertificateModel model) =>
        model.Certificate.Extensions
            .OfType<System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension>()
            .Any(eku => eku.EnhancedKeyUsages
                .Cast<System.Security.Cryptography.Oid>()
                .Any(o => o.Value == "1.3.6.1.5.5.7.3.3")); // Code Signing EKU

    public static bool IsRootCertificate(this CertificateModel model) =>
        model.Certificate.Subject == model.Certificate.Issuer;
}

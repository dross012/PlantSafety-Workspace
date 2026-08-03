using System.Security.Cryptography.X509Certificates;

namespace PlantClientInstaller.Core.Payload.Certificates;

public sealed class CertificateModel
{
    public string Name { get; }
    public string FilePath { get; }
    public X509Certificate2 Certificate { get; }

    public CertificateModel(string name, string filePath, X509Certificate2 certificate)
    {
        Name = name;
        FilePath = filePath;
        Certificate = certificate;
    }
}

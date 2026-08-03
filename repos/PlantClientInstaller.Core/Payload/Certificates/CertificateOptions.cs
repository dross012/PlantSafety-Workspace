namespace PlantClientInstaller.Core.Payload.Certificates;

public sealed class CertificateOptions
{
    public string CertificateDirectory { get; }
    public string ManifestFileName { get; }

    public CertificateOptions(string certificateDirectory, string manifestFileName = "certificates.manifest.json")
    {
        CertificateDirectory = certificateDirectory;
        ManifestFileName = manifestFileName;
    }

    public string ManifestPath => Path.Combine(CertificateDirectory, ManifestFileName);
}

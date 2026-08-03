namespace PlantClientInstaller.Core.Payload.Certificates;

public interface ICertificateProvider
{
    IReadOnlyList<CertificateModel> LoadAll();
    bool TryLoadAll(out IReadOnlyList<CertificateModel>? certificates);
    CertificateModel Get(string certificateName);
}

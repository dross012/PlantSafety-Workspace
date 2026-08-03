using System.Security.Cryptography.X509Certificates;

namespace PlantClientInstaller.Core.Payload.Certificates;

public sealed class CertificateProvider : ICertificateProvider
{
    private readonly CertificateLoader _loader;
    private readonly CertificateValidator _validator;
    private readonly CertificatePolicy _policy;
    private readonly CertificateOptions _options;

    public CertificateProvider(
        CertificateLoader loader,
        CertificateValidator validator,
        CertificatePolicy policy,
        CertificateOptions options)
    {
        _loader = loader;
        _validator = validator;
        _policy = policy;
        _options = options;
    }

    public IReadOnlyList<CertificateModel> LoadAll()
    {
        var manifest = _loader.LoadManifest();
        var list = new List<CertificateModel>();

        foreach (var entry in manifest)
        {
            var filePath = Path.Combine(_options.CertificateDirectory, entry.File);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Certificate file missing: {filePath}");

            var cert = new X509Certificate2(filePath);

            _validator.Validate(cert, _policy);

            list.Add(new CertificateModel(entry.Name, filePath, cert));
        }

        return list;
    }

    public bool TryLoadAll(out IReadOnlyList<CertificateModel>? certificates)
    {
        try
        {
            certificates = LoadAll();
            return true;
        }
        catch
        {
            certificates = null;
            return false;
        }
    }

    public CertificateModel Get(string certificateName)
    {
        var all = LoadAll();
        return all.FirstOrDefault(c => c.Name.Equals(certificateName, StringComparison.OrdinalIgnoreCase))
               ?? throw new KeyNotFoundException($"Certificate '{certificateName}' not found.");
    }
}

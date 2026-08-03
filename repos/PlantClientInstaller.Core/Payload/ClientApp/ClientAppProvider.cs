namespace PlantClientInstaller.Core.Payload.ClientApp;

public sealed class ClientAppProvider : IClientAppProvider
{
    private readonly ClientAppManifestLoader _loader;
    private readonly ClientAppValidator _validator;
    private readonly ClientAppPolicy _policy;
    private readonly ClientAppOptions _options;

    public ClientAppProvider(
        ClientAppManifestLoader loader,
        ClientAppValidator validator,
        ClientAppPolicy policy,
        ClientAppOptions options)
    {
        _loader = loader;
        _validator = validator;
        _policy = policy;
        _options = options;
    }

    public ClientAppModel Load()
    {
        var manifest = _loader.Load();
        _validator.Validate(manifest, _policy, _options.AppDirectory);

        var exePath = Path.Combine(_options.AppDirectory, manifest.Executable);

        return new ClientAppModel(
            manifest.Name,
            exePath,
            manifest.Version,
            manifest.Dependencies.AsReadOnly()
        );
    }

    public bool TryLoad(out ClientAppModel? model)
    {
        try
        {
            model = Load();
            return true;
        }
        catch
        {
            model = null;
            return false;
        }
    }
}

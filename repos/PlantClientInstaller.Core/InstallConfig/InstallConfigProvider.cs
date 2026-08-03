namespace PlantClientInstaller.Core.InstallConfig;

public sealed class InstallConfigProvider : IInstallConfigProvider
{
    private readonly InstallConfigLoader _loader;
    private readonly InstallConfigValidator _validator;
    private readonly InstallConfigPolicy _policy;

    public InstallConfigProvider(
        InstallConfigLoader loader,
        InstallConfigValidator validator,
        InstallConfigPolicy policy)
    {
        _loader = loader;
        _validator = validator;
        _policy = policy;
    }

    public InstallConfigModel Load()
    {
        var config = _loader.Load();
        _validator.Validate(config, _policy);
        return config;
    }

    public bool TryLoad(out InstallConfigModel? config)
    {
        try
        {
            config = Load();
            return true;
        }
        catch
        {
            config = null;
            return false;
        }
    }
}

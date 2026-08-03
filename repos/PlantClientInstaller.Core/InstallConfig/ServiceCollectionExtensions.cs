using Microsoft.Extensions.DependencyInjection;

namespace PlantClientInstaller.Core.InstallConfig;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstallerConfig(
        this IServiceCollection services,
        InstallConfigOptions options,
        InstallConfigPolicy? policy = null)
    {
        services.AddSingleton(options);
        services.AddSingleton(policy ?? new InstallConfigPolicy());
        services.AddSingleton<InstallConfigValidator>();
        services.AddSingleton<InstallConfigLoader>();
        services.AddSingleton<IInstallConfigProvider, InstallConfigProvider>();

        return services;
    }
}

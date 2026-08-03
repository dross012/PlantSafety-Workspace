using Microsoft.Extensions.DependencyInjection;

namespace PlantClientInstaller.Core.Payload.ClientApp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstallerClientApp(
        this IServiceCollection services,
        ClientAppOptions options,
        ClientAppPolicy? policy = null)
    {
        services.AddSingleton(options);
        services.AddSingleton(policy ?? new ClientAppPolicy());
        services.AddSingleton<ClientAppValidator>();
        services.AddSingleton<ClientAppManifestLoader>();
        services.AddSingleton<IClientAppProvider, ClientAppProvider>();

        return services;
    }
}

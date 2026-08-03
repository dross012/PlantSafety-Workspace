using Microsoft.Extensions.DependencyInjection;

namespace PlantClientInstaller.Core.Payload.Assets;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstallerAssets(
        this IServiceCollection services,
        AssetOptions options,
        AssetPolicy? policy = null)
    {
        services.AddSingleton(options);
        services.AddSingleton(policy ?? new AssetPolicy());
        services.AddSingleton<AssetValidator>();
        services.AddSingleton<AssetManifestLoader>();
        services.AddSingleton<IAssetProvider, AssetProvider>();

        return services;
    }
}

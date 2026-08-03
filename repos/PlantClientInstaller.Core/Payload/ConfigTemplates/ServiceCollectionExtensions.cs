using Microsoft.Extensions.DependencyInjection;

namespace PlantClientInstaller.Core.Payload.ConfigTemplates;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstallerConfigTemplates(
        this IServiceCollection services,
        ConfigTemplateOptions options,
        ConfigTemplatePolicy? policy = null)
    {
        services.AddSingleton(options);
        services.AddSingleton(policy ?? new ConfigTemplatePolicy());
        services.AddSingleton<ConfigTemplateValidator>();
        services.AddSingleton<ConfigTemplateManifestLoader>();
        services.AddSingleton<IConfigTemplateProvider, ConfigTemplateProvider>();
        services.AddSingleton<ConfigTemplateRenderer>();

        return services;
    }
}

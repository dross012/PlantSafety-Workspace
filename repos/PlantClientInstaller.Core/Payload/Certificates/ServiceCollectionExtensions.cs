using Microsoft.Extensions.DependencyInjection;

namespace PlantClientInstaller.Core.Payload.Certificates;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstallerCertificates(
        this IServiceCollection services,
        CertificateOptions options,
        CertificatePolicy? policy = null)
    {
        services.AddSingleton(options);
        services.AddSingleton(policy ?? new CertificatePolicy());
        services.AddSingleton<CertificateValidator>();
        services.AddSingleton<CertificateLoader>();
        services.AddSingleton<ICertificateProvider, CertificateProvider>();

        return services;
    }
}

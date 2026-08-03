using Microsoft.Extensions.DependencyInjection;

namespace PlantClientInstaller.Core.Logs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInstallerLogging(
        this IServiceCollection services,
        LogFileOptions fileOptions,
        LogPolicy? policy = null)
    {
        services.AddSingleton(fileOptions);
        services.AddSingleton(policy ?? new LogPolicy());
        services.AddSingleton<LogFileProvider>();
        services.AddSingleton<ILogWriter, LogWriter>();
        services.AddSingleton<ILogReader, LogReader>();

        return services;
    }
}

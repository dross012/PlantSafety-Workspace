// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Registers LogisticsTransportSafety module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.LogisticsTransportSafety.DependencyInjection;

/// <summary>
/// Extension methods to register LogisticsTransportSafety Application-layer services.
/// </summary>
public static class LogisticsTransportSafetyApplicationServiceRegistration
{
    /// <summary>
    /// Registers LogisticsTransportSafety-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddLogisticsTransportSafetyApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

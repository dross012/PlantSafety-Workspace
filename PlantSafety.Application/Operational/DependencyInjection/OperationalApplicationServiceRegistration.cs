// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Registers Operational module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Operational.DependencyInjection;

/// <summary>
/// Extension methods to register Operational Application-layer services.
/// </summary>
public static class OperationalApplicationServiceRegistration
{
    /// <summary>
    /// Registers Operational-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddOperationalApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

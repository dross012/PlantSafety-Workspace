// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Registers EnterpriseFabric module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.EnterpriseFabric.DependencyInjection;

/// <summary>
/// Extension methods to register EnterpriseFabric Application-layer services.
/// </summary>
public static class EnterpriseFabricApplicationServiceRegistration
{
    /// <summary>
    /// Registers EnterpriseFabric-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddEnterpriseFabricApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

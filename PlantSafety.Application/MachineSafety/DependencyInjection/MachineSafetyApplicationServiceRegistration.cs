// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers MachineSafety module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.MachineSafety.DependencyInjection;

/// <summary>
/// Extension methods to register MachineSafety Application-layer services.
/// </summary>
public static class MachineSafetyApplicationServiceRegistration
{
    /// <summary>
    /// Registers MachineSafety-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddMachineSafetyApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
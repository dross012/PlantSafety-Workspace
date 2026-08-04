// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Registers AutonomousVehicleSafety module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.AutonomousVehicleSafety.DependencyInjection;

/// <summary>
/// Extension methods to register AutonomousVehicleSafety Application-layer services.
/// </summary>
public static class AutonomousVehicleSafetyApplicationServiceRegistration
{
    /// <summary>
    /// Registers AutonomousVehicleSafety-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddAutonomousVehicleSafetyApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers PermitToWork module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.PermitToWork.DependencyInjection;

/// <summary>
/// Extension methods to register PermitToWork Application-layer services.
/// </summary>
public static class PermitToWorkApplicationServiceRegistration
{
    /// <summary>
    /// Registers PermitToWork-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddPermitToWorkApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
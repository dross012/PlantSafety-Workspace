// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Registers PhysicalAccessControl module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.PhysicalAccessControl.DependencyInjection;

/// <summary>
/// Extension methods to register PhysicalAccessControl Application-layer services.
/// </summary>
public static class PhysicalAccessControlApplicationServiceRegistration
{
    /// <summary>
    /// Registers PhysicalAccessControl-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddPhysicalAccessControlApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

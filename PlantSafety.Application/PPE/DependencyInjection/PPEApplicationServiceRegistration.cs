// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers PPE module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.PPE.DependencyInjection;

/// <summary>
/// Extension methods to register PPE Application-layer services.
/// </summary>
public static class PPEApplicationServiceRegistration
{
    /// <summary>
    /// Registers PPE-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddPPEApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
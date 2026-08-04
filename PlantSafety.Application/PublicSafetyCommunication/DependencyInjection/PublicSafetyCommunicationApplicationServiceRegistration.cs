// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Registers PublicSafetyCommunication module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.PublicSafetyCommunication.DependencyInjection;

/// <summary>
/// Extension methods to register PublicSafetyCommunication Application-layer services.
/// </summary>
public static class PublicSafetyCommunicationApplicationServiceRegistration
{
    /// <summary>
    /// Registers PublicSafetyCommunication-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddPublicSafetyCommunicationApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

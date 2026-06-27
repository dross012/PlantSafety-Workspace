// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Registers Compliance module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Compliance.DependencyInjection;

/// <summary>
/// Extension methods to register Compliance Application-layer services.
/// </summary>
public static class ComplianceApplicationServiceRegistration
{
    /// <summary>
    /// Registers Compliance-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddComplianceApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

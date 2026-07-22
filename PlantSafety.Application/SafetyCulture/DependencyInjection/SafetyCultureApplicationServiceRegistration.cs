// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers SafetyCulture module Application-layer services. Self-contained extension method; wired into the composition root by the host application. Not auto-linked into the shared ApplicationServiceRegistration entry point to avoid cross-agent merge collisions during the legacy-module scaffolding effort -- see module README.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.SafetyCulture.DependencyInjection;

/// <summary>
/// Extension methods to register SafetyCulture Application-layer services.
/// </summary>
public static class SafetyCultureApplicationServiceRegistration
{
    /// <summary>
    /// Registers SafetyCulture-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddSafetyCultureApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
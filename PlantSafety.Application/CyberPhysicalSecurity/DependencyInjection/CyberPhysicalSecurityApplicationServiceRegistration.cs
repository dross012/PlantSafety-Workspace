// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Registers CyberPhysicalSecurity module Application-layer services.
//   Self-contained extension method; wired into the composition root by the host application per
//   the RiskManagement/PermitToWork precedent -- NOT auto-linked into the shared
//   ApplicationServiceRegistration entry point to avoid cross-agent merge collisions.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.CyberPhysicalSecurity.DependencyInjection;

/// <summary>
/// Extension methods to register CyberPhysicalSecurity Application-layer services.
/// </summary>
public static class CyberPhysicalSecurityApplicationServiceRegistration
{
    /// <summary>
    /// Registers CyberPhysicalSecurity-specific Application services.
    /// MediatR handlers and FluentValidation validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddCyberPhysicalSecurityApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

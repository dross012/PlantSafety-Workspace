// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers SafetyPolicies module Application-layer services. Self-contained extension method; wired into the composition root by the host application. Not auto-linked into the shared ApplicationServiceRegistration entry point to avoid cross-agent merge collisions during the legacy-module scaffolding effort -- see module README.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.SafetyPolicies.DependencyInjection;

/// <summary>
/// Extension methods to register SafetyPolicies Application-layer services.
/// </summary>
public static class SafetyPoliciesApplicationServiceRegistration
{
    /// <summary>
    /// Registers SafetyPolicies-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddSafetyPoliciesApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Registers SafetyPrograms module Application-layer services. Self-contained
//   extension method; wired into the composition root by the host application. Mirrors
//   SafetyCultureApplicationServiceRegistration -- MediatR handlers and validators are
//   auto-registered via assembly scan, so this method is currently a no-op placeholder kept
//   for structural symmetry with sibling modules.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.SafetyPrograms.DependencyInjection;

/// <summary>
/// Extension methods to register SafetyPrograms Application-layer services.
/// </summary>
public static class SafetyProgramsApplicationServiceRegistration
{
    /// <summary>
    /// Registers SafetyPrograms-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddSafetyProgramsApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

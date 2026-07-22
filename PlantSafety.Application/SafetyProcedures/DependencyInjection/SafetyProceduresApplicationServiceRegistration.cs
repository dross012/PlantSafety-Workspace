// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Registers SafetyProcedures module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.SafetyProcedures.DependencyInjection;

/// <summary>
/// Extension methods to register SafetyProcedures Application-layer services.
/// </summary>
public static class SafetyProceduresApplicationServiceRegistration
{
    /// <summary>
    /// Registers SafetyProcedures-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddSafetyProceduresApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}
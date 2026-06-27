// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Registers ContinuousImprovement module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.ContinuousImprovement.DependencyInjection;

/// <summary>
/// Extension methods to register ContinuousImprovement Application-layer services.
/// </summary>
public static class ContinuousImprovementApplicationServiceRegistration
{
    /// <summary>
    /// Registers ContinuousImprovement-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddContinuousImprovementApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

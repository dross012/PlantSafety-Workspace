// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Registers Financials module Application-layer services.
// SchemaVersion: 1

using Microsoft.Extensions.DependencyInjection;

namespace PlantSafety.Application.Financials.DependencyInjection;

/// <summary>
/// Extension methods to register Financials Application-layer services.
/// </summary>
public static class FinancialsApplicationServiceRegistration
{
    /// <summary>
    /// Registers Financials-specific Application services.
    /// MediatR handlers and validators are auto-registered via assembly scan.
    /// </summary>
    public static IServiceCollection AddFinancialsApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.SafetyEvents
//  Layer:          Infrastructure
//  File:           SafetyEventsRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all SafetyEvents repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.SafetyEvents;

/// <summary>
/// Extension method that registers all <c>SafetyEvents</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class SafetyEventsRepositoryRegistration
{
    /// <summary>Registers all SafetyEvents Dapper repositories.</summary>
    public static IServiceCollection AddSafetyEventsRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

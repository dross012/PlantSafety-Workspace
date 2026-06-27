// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.Intergrations
//  Layer:          Infrastructure
//  File:           IntergrationsRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all Intergrations repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.Intergrations;

/// <summary>
/// Extension method that registers all <c>Intergrations</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class IntergrationsRepositoryRegistration
{
    /// <summary>Registers all Intergrations Dapper repositories.</summary>
    public static IServiceCollection AddIntergrationsRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.EnvironmentalHealth
//  Layer:          Infrastructure
//  File:           EnvironmentalHealthRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all EnvironmentalHealth repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.EnvironmentalHealth;

/// <summary>
/// Extension method that registers all <c>EnvironmentalHealth</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class EnvironmentalHealthRepositoryRegistration
{
    /// <summary>Registers all EnvironmentalHealth Dapper repositories.</summary>
    public static IServiceCollection AddEnvironmentalHealthRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

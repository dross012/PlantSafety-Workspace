// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.EnterpriseFabric
//  Layer:          Infrastructure
//  File:           EnterpriseFabricRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all EnterpriseFabric repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.EnterpriseFabric;

/// <summary>
/// Extension method that registers all <c>EnterpriseFabric</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class EnterpriseFabricRepositoryRegistration
{
    /// <summary>Registers all EnterpriseFabric Dapper repositories.</summary>
    public static IServiceCollection AddEnterpriseFabricRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

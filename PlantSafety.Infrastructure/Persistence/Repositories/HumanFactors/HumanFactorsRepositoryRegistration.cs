// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.HumanFactors
//  Layer:          Infrastructure
//  File:           HumanFactorsRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all HumanFactors repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.HumanFactors;

/// <summary>
/// Extension method that registers all <c>HumanFactors</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class HumanFactorsRepositoryRegistration
{
    /// <summary>Registers all HumanFactors Dapper repositories.</summary>
    public static IServiceCollection AddHumanFactorsRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.Sustainability
//  Layer:          Infrastructure
//  File:           SustainabilityRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all Sustainability repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.Sustainability;

/// <summary>
/// Extension method that registers all <c>Sustainability</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class SustainabilityRepositoryRegistration
{
    /// <summary>Registers all Sustainability Dapper repositories.</summary>
    public static IServiceCollection AddSustainabilityRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

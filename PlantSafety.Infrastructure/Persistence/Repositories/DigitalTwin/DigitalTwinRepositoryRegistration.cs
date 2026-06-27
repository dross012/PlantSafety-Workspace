// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.DigitalTwin
//  Layer:          Infrastructure
//  File:           DigitalTwinRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all DigitalTwin repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.DigitalTwin;

/// <summary>
/// Extension method that registers all <c>DigitalTwin</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class DigitalTwinRepositoryRegistration
{
    /// <summary>Registers all DigitalTwin Dapper repositories.</summary>
    public static IServiceCollection AddDigitalTwinRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

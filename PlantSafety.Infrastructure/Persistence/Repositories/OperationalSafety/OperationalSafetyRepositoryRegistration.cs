// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.OperationalSafety
//  Layer:          Infrastructure
//  File:           OperationalSafetyRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all OperationalSafety repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.OperationalSafety;

/// <summary>
/// Extension method that registers all <c>OperationalSafety</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class OperationalSafetyRepositoryRegistration
{
    /// <summary>Registers all OperationalSafety Dapper repositories.</summary>
    public static IServiceCollection AddOperationalSafetyRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

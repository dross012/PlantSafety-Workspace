// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.CrossDomain
//  Layer:          Infrastructure
//  File:           CrossDomainRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all CrossDomain repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.CrossDomain;

/// <summary>
/// Extension method that registers all <c>CrossDomain</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class CrossDomainRepositoryRegistration
{
    /// <summary>Registers all CrossDomain Dapper repositories.</summary>
    public static IServiceCollection AddCrossDomainRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

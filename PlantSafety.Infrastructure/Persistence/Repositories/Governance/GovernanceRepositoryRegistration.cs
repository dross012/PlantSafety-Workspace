// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.Governance
//  Layer:          Infrastructure
//  File:           GovernanceRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all Governance repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.Governance;

/// <summary>
/// Extension method that registers all <c>Governance</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class GovernanceRepositoryRegistration
{
    /// <summary>Registers all Governance Dapper repositories.</summary>
    public static IServiceCollection AddGovernanceRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

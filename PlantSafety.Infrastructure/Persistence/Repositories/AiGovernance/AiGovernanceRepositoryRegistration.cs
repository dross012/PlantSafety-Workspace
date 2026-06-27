// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.AiGovernance
//  Layer:          Infrastructure
//  File:           AiGovernanceRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all AiGovernance repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.AiGovernance;

/// <summary>
/// Extension method that registers all <c>AiGovernance</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class AiGovernanceRepositoryRegistration
{
    /// <summary>Registers all AiGovernance Dapper repositories.</summary>
    public static IServiceCollection AddAiGovernanceRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

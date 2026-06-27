// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.AiSafetyIntelligence
//  Layer:          Infrastructure
//  File:           AiSafetyIntelligenceRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all AiSafetyIntelligence repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.AiSafetyIntelligence;

/// <summary>
/// Extension method that registers all <c>AiSafetyIntelligence</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class AiSafetyIntelligenceRepositoryRegistration
{
    /// <summary>Registers all AiSafetyIntelligence Dapper repositories.</summary>
    public static IServiceCollection AddAiSafetyIntelligenceRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

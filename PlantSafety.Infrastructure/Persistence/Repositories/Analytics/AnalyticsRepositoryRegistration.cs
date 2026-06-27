// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.Analytics
//  Layer:          Infrastructure
//  File:           AnalyticsRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all Analytics repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.Analytics;

/// <summary>
/// Extension method that registers all <c>Analytics</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class AnalyticsRepositoryRegistration
{
    /// <summary>Registers all Analytics Dapper repositories.</summary>
    public static IServiceCollection AddAnalyticsRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

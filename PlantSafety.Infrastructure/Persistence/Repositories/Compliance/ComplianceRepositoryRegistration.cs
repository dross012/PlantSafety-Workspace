// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.Compliance
//  Layer:          Infrastructure
//  File:           ComplianceRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all Compliance repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.Compliance;

/// <summary>
/// Extension method that registers all <c>Compliance</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class ComplianceRepositoryRegistration
{
    /// <summary>Registers all Compliance Dapper repositories.</summary>
    public static IServiceCollection AddComplianceRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

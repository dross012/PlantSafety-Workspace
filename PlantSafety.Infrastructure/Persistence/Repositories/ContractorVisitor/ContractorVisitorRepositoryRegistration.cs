// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.ContractorVisitor
//  Layer:          Infrastructure
//  File:           ContractorVisitorRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all ContractorVisitor repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.ContractorVisitor;

/// <summary>
/// Extension method that registers all <c>ContractorVisitor</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class ContractorVisitorRepositoryRegistration
{
    /// <summary>Registers all ContractorVisitor Dapper repositories.</summary>
    public static IServiceCollection AddContractorVisitorRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

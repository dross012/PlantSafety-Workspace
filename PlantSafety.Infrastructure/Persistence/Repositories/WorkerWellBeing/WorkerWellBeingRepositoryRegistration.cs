// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.WorkerWellBeing
//  Layer:          Infrastructure
//  File:           WorkerWellBeingRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all WorkerWellBeing repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.WorkerWellBeing;

/// <summary>
/// Extension method that registers all <c>WorkerWellBeing</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class WorkerWellBeingRepositoryRegistration
{
    /// <summary>Registers all WorkerWellBeing Dapper repositories.</summary>
    public static IServiceCollection AddWorkerWellBeingRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

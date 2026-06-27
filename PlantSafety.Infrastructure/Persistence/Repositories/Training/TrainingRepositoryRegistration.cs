// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.Training
//  Layer:          Infrastructure
//  File:           TrainingRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all Training repositories.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Persistence.Repositories.Training;

/// <summary>
/// Extension method that registers all <c>Training</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class TrainingRepositoryRegistration
{
    /// <summary>Registers all Training Dapper repositories.</summary>
    public static IServiceCollection AddTrainingRepositories(this IServiceCollection services)
    {
        // TODO: services.AddScoped<I{Aggregate}Repository, {Aggregate}DapperRepository>();
        return services;
    }
}

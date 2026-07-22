// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HealthChecks.DependencyInjection
//  Layer:          Infrastructure
//  File:           HealthChecksRepositoryRegistration.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration entry point for the HealthChecks read-model repository,
//                  newly implemented end-to-end (SQL table + stored procedure + Dapper
//                  repository) in this pass under the [HealthChecks] SQL schema.
//                  Call <c>services.AddHealthChecksRepositories()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per
//                  task constraints, that central file is intentionally not edited here to avoid
//                  concurrent-edit collisions with other agents). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.HealthChecks.Repositories;
using PlantSafety.Infrastructure.HealthChecks.Persistence;

namespace PlantSafety.Infrastructure.HealthChecks.DependencyInjection;

/// <summary>
/// Per-module DI entry point for the HealthChecks read-model repository.
/// </summary>
public static class HealthChecksRepositoryRegistration
{
    /// <summary>Registers the HealthChecks Dapper repository into the DI container.</summary>
    public static IServiceCollection AddHealthChecksRepositories(this IServiceCollection services)
    {
        services.AddScoped<IHealthChecksRepository, HealthChecksDapperRepository>();

        return services;
    }
}

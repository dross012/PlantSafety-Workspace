// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects.DependencyInjection
//  Layer:          Infrastructure
//  File:           PlanningProjectsRepositoryRegistration.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    DI registration entry point for the PlanningProjects read-model repository,
//                  newly implemented end-to-end (SQL table + stored procedure + Dapper
//                  repository) in this pass under the [PlanningProjects] SQL schema.
//                  Call <c>services.AddPlanningProjectsRepositories()</c> from host startup
//                  alongside <c>services.AddInfrastructure(configuration)</c>. NOT invoked by the
//                  shared InfrastructureServiceRegistration.AddInfrastructure() yet -- wiring the
//                  call site there is a required, tracked follow-up for a human maintainer (per
//                  task constraints, that central file is intentionally not edited here to avoid
//                  concurrent-edit collisions with other agents). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PlanningProjects.PlanningProjects.Repositories;
using PlantSafety.Infrastructure.PlanningProjects.Persistence;

namespace PlantSafety.Infrastructure.PlanningProjects.DependencyInjection;

/// <summary>
/// Per-module DI entry point for the PlanningProjects read-model repository.
/// </summary>
public static class PlanningProjectsRepositoryRegistration
{
    /// <summary>Registers the PlanningProjects Dapper repository into the DI container.</summary>
    public static IServiceCollection AddPlanningProjectsRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPlanningProjectsRepository, PlanningProjectsDapperRepository>();

        return services;
    }
}

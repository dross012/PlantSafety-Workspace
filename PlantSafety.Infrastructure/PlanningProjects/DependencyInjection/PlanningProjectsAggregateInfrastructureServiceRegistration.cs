// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Infrastructure
//  File:           PlanningProjectsAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Phase 4 full build-out write-side
//                  aggregate repository implementation for PlanningProjects
//                  (ProjectAggregateDapperRepository implementing the Domain-layer
//                  IProjectRepository write-side interface) using a fully-qualified name to avoid
//                  an ambiguous-reference compile error against the pre-existing, unrelated,
//                  same-simple-name Application.Features.PlanningProjects.Handlers.Projects
//                  .IProjectRepository interface, exactly like
//                  SafetyEventsNearMissesRootCauseAnalysisInfrastructureServiceRegistration.cs's
//                  identical precedent. Wired into the central
//                  InfrastructureServiceRegistration.cs directly.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.PlanningProjects.Persistence;

namespace PlantSafety.Infrastructure.PlanningProjects.DependencyInjection;

/// <summary>Registers the Infrastructure-layer write-side aggregate repository for the PlanningProjects module.</summary>
public static class PlanningProjectsAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds the Project aggregate's write-side Dapper repository to the service collection.</summary>
    public static IServiceCollection AddPlanningProjectsAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.PlanningProjects.Interfaces.IProjectRepository, ProjectAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.PlanningProjects.ProjectAggregate.Repositories.IProjectReadRepository, ProjectReadDapperRepository>();

        return services;
    }
}

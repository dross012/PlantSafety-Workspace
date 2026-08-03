// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Infrastructure
//  File:           WorkflowInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the Workflow Infrastructure-layer
//                  repository implementations, mirroring
//                  RiskManagementInfrastructureServiceRegistration's identical precedent
//                  (write-side Domain repository interfaces + read-side Application repository
//                  interfaces, both implemented here).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Workflow.Repositories;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Infrastructure.Workflow.Persistence;

namespace PlantSafety.Infrastructure.Workflow.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the Workflow module.</summary>
public static class WorkflowInfrastructureServiceRegistration
{
    /// <summary>Adds Workflow Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddWorkflowInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionDapperRepository>();
        services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceDapperRepository>();

        services.AddScoped<IWorkflowDefinitionReadRepository, WorkflowDefinitionReadDapperRepository>();
        services.AddScoped<IWorkflowInstanceReadRepository, WorkflowInstanceReadDapperRepository>();

        return services;
    }
}

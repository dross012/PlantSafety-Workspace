// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Infrastructure
//  File:           ProcessManagementAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the ProcessManagement Infrastructure-layer
//                  repository implementations for the ProcessDefinition aggregate. Registers both
//                  same-simple-name IProcessDefinitionRepository interfaces (Domain write-side ->
//                  ProcessDefinitionAggregateDapperRepository, Application read-side ->
//                  ProcessDefinitionReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error. The extension method is deliberately named
//                  AddProcessManagementAggregateInfrastructure (not AddProcessManagementInfrastructure)
//                  to avoid colliding with any pre-existing Features-vertical registration for
//                  this module. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.DependencyInjection.ComplianceAuditingInfrastructureServiceRegistration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.ProcessManagement.Persistence;

namespace PlantSafety.Infrastructure.ProcessManagement.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the ProcessManagement (ProcessDefinition aggregate) module.</summary>
public static class ProcessManagementAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds ProcessDefinition aggregate Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddProcessManagementAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.ProcessManagement.Repositories.IProcessDefinitionRepository, ProcessDefinitionAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.ProcessManagement.Repositories.IProcessDefinitionRepository, ProcessDefinitionReadDapperRepository>();

        return services;
    }
}

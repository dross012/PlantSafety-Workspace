// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Infrastructure
//  File:           WorkerWellBeingAggregateInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the WorkerWellBeing Infrastructure-layer
//                  repository implementations for the WorkerWellBeing aggregate. Registers both
//                  same-simple-name IWorkerWellBeingRepository interfaces (Domain write-side ->
//                  WorkerWellBeingAggregateDapperRepository, Application read-side ->
//                  WorkerWellBeingReadDapperRepository) using fully-qualified names to avoid an
//                  ambiguous-reference compile error. The extension method is deliberately named
//                  AddWorkerWellBeingAggregateInfrastructure (not AddWorkerWellBeingInfrastructure)
//                  to avoid colliding with the pre-existing Features-vertical
//                  IWorkerWellBeingRecordRepository registration for this module. Mirrors
//                  PlantSafety.Infrastructure.Compliance.Auditing.DependencyInjection.ComplianceAuditingInfrastructureServiceRegistration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Infrastructure.WorkerWellBeing.Persistence;

namespace PlantSafety.Infrastructure.WorkerWellBeing.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the WorkerWellBeing (WorkerWellBeing aggregate) module.</summary>
public static class WorkerWellBeingAggregateInfrastructureServiceRegistration
{
    /// <summary>Adds WorkerWellBeing aggregate Dapper repositories to the service collection.</summary>
    public static IServiceCollection AddWorkerWellBeingAggregateInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<PlantSafety.Domain.WorkerWellBeing.Repositories.IWorkerWellBeingRepository, WorkerWellBeingAggregateDapperRepository>();
        services.AddScoped<PlantSafety.Application.WorkerWellBeing.Repositories.IWorkerWellBeingRepository, WorkerWellBeingReadDapperRepository>();

        return services;
    }
}

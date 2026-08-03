// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Infrastructure
//  File:           AuditTimelineInfrastructureServiceRegistration.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Per-module DI extension registering the AuditTimeline Infrastructure-layer
//                  repository implementations. NOT wired into the shared
//                  InfrastructureServiceRegistration.cs -- a human wires this extension in
//                  centrally after all module-scoped agents finish. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditTimeline.Interfaces;
using PlantSafety.Application.Auditing.AuditTimeline.Repositories;
using PlantSafety.Infrastructure.Auditing.AuditTimeline.Persistence;

namespace PlantSafety.Infrastructure.Auditing.AuditTimeline.DependencyInjection;

/// <summary>Registers Infrastructure-layer services for the AuditTimeline submodule.</summary>
public static class AuditTimelineInfrastructureServiceRegistration
{
    /// <summary>Adds AuditTimeline Dapper repositories and Outbox-backed providers to the service collection.</summary>
    public static IServiceCollection AddAuditTimelineInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IConcurrencyConflictRepository, ConcurrencyConflictDapperRepository>();

        services.AddScoped<OutboxTimelineEntryReader>();
        services.AddScoped<IAuditTimelineEventSourceRepository, AuditTimelineEventSourceOutboxRepository>();
        services.AddScoped<IAuditTimelineStateChangeRepository, AuditTimelineStateChangeOutboxRepository>();
        services.AddScoped<ISchedulingDecisionHistoryProvider, SchedulingDecisionOutboxProvider>();

        return services;
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Infrastructure
//  File:           GlobalSafetyTelemetryConsumer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Idempotent MassTransit consumer wiring a real per-site domain event into the
//                  GlobalSafetyInsights compute pipeline, following the exact
//                  CrossModuleIdempotentConsumerBase pattern established by Phase 5 §3 Cross-Module
//                  Orchestration (see RiskEscalationCapaConsumer). Consumes
//                  IncidentManagementCreatedEvent (SafetyEvents.IncidentManagement -- the canonical
//                  incident aggregate this build's task brief singles out as extended with SLA
//                  monitoring) as the primary real-time trigger: incidents are the most direct,
//                  unambiguous "a safety event just happened at site X" signal available today.
//                  IGlobalSafetyTelemetryNormalizer already exposes real Normalize(...) overloads
//                  for AccidentManagement, Observability.Telemetry, EnvironmentalHealthMonitoring,
//                  and ProductionTracking sources too (see that interface's remarks) -- additional
//                  sibling consumers following this exact same shape (one IConsumer&lt;TSourceEvent&gt;
//                  per real source type) are the documented, ready-to-add extension seam for wiring
//                  those sources into live event-driven computation; not wired here to keep this
//                  pass's real, verified surface area honest rather than fabricating consumers this
//                  pass cannot integration-test end-to-end. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Application.GlobalSafetyInsights.Commands;
using PlantSafety.Domain.GlobalSafetyInsights.Interfaces;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;
using PlantSafety.Infrastructure.Messaging;
using PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.GlobalSafetyInsights.Messaging.Consumers;

/// <summary>
/// Idempotent consumer that normalizes a real IncidentManagementCreatedEvent and runs the
/// GlobalSafetyInsights compute pipeline for the incident's site.
/// </summary>
public sealed class GlobalSafetyTelemetryConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IIncidentReadRepository incidentReadRepository,
    IGlobalSafetyTelemetryNormalizer normalizer,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<GlobalSafetyTelemetryConsumer> logger)
    : CrossModuleIdempotentConsumerBase<IncidentManagementCreatedEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    /// <summary>Rolling analysis window applied to each single-incident-triggered computation.</summary>
    private static readonly TimeSpan AnalysisWindow = TimeSpan.FromHours(24);

    /// <summary>Severity values treated as lost-time incidents for LTIR purposes, pending a dedicated LostTime flag on the Incident read-model.</summary>
    private static readonly HashSet<string> LostTimeSeverities = new(StringComparer.OrdinalIgnoreCase) { "Critical", "Major" };

    /// <inheritdoc/>
    protected override Guid ResolveEventId(IncidentManagementCreatedEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<IncidentManagementCreatedEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;
        var tenantId = context.GetEnvelopeTenantId();

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "GlobalSafetyTelemetryConsumer: no TenantId header on incident {IncidentId}; skipping computation.",
                evt.IncidentManagementId);
            return;
        }

        var incident = await incidentReadRepository.GetByIdAsync(tenantId, evt.IncidentManagementId.Value, cancellationToken);
        if (incident is null)
        {
            Logger.LogWarning(
                "GlobalSafetyTelemetryConsumer: incident {IncidentId} not found for Tenant {TenantId}; skipping computation.",
                evt.IncidentManagementId, tenantId);
            return;
        }

        var isLostTime = LostTimeSeverities.Contains(incident.Severity);
        var telemetry = normalizer.Normalize(evt, tenantId, incident.SiteId, isLostTime);

        var command = new ComputeGlobalSafetyInsightsCommand(
            TenantId: tenantId,
            SiteIds: [incident.SiteId],
            WindowStartUtc: evt.CreatedAtUtc.Value - AnalysisWindow,
            WindowEndUtc: evt.CreatedAtUtc.Value,
            Telemetry: [telemetry],
            ActorUserId: null,
            CorrelationId: context.CorrelationId ?? Guid.NewGuid(),
            CausationId: evt.EventId.Value);

        await DispatchAsync(command, $"ComputeGlobalSafetyInsights for incident {evt.IncidentManagementId}", cancellationToken);
    }
}

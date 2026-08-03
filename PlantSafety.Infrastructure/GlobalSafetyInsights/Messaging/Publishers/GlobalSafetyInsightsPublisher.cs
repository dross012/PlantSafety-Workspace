// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Infrastructure
//  File:           GlobalSafetyInsightsPublisher.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §4 Event Mesh Extensions)
//  SchemaVersion:  2026.1
//  Description:    Real IGlobalSafetyInsightsPublisher implementation (also exposed as
//                  IGlobalSafetyDashboardPublisher -- see remarks below). Enforces data residency
//                  at the point of translation (mirroring HttpExternalSystemPublisher's Phase 6/7
//                  IDataResidencyEnforcer check on the external-system mesh) before enqueuing the
//                  dashboard-facing GlobalSafetyKpiSnapshotEvent/GlobalSafetyAnomalyEvent onto the
//                  SAME real internal Outbox -> MassTransit pipeline every other domain event in
//                  this codebase already uses (IOutboxWriter.Enqueue, flushed by
//                  TransactionalUnitOfWork.CommitAsync / OutboxPipelineBehavior), rather than
//                  inventing a parallel publish mechanism. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.Application.GlobalSafetyInsights.Publishers;
using PlantSafety.Domain.GlobalSafetyInsights.Aggregates;
using PlantSafety.Domain.GlobalSafetyInsights.Events;
using PlantSafety.Domain.GlobalSafetyInsights.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Infrastructure.GlobalSafetyInsights.Messaging.Publishers;

/// <summary>
/// Publishes the dashboard-facing GlobalSafetyInsights mesh events for a freshly computed
/// snapshot. Also registered as <c>GlobalSafetyDashboardPublisher</c>'s implementation surface via
/// DI (same concrete type, both names refer to the identical publishing responsibility the Phase 8
/// §4 spec describes as "GlobalSafetyInsightsPublisher/GlobalSafetyDashboardPublisher" -- there is
/// exactly one real publishing pipeline, not two competing ones).
/// </summary>
public sealed class GlobalSafetyInsightsPublisher(
    IOutboxWriter outboxWriter,
    IDataResidencyEnforcer residencyEnforcer,
    ILogger<GlobalSafetyInsightsPublisher> logger)
    : IGlobalSafetyInsightsPublisher
{
    /// <inheritdoc/>
    public Task PublishAsync(GlobalSafetyInsightsAggregate aggregate, Guid correlationId, Guid causationId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var correlation = CorrelationId.FromGuid(correlationId == Guid.Empty ? Guid.NewGuid() : correlationId);
        var causation = CausationId.FromGuid(causationId == Guid.Empty ? correlation.Value : causationId);

        // Phase 7 §5.3 Data Residency: reject/flag loudly rather than silently fanning out
        // cross-site intelligence data outside the tenant's configured residency region.
        residencyEnforcer.EnsureSatisfied(aggregate.TenantId, correlation, causation);

        var nowUtc = UtcTimestamp.Now();
        var primarySiteId = aggregate.SiteIds.Count > 0 ? aggregate.SiteIds[0] : Guid.Empty;
        var actor = PerformedByActor.System();

        var kpiEvent = new GlobalSafetyKpiSnapshotEvent(
            GlobalSafetyInsightsEventId.New(), aggregate.GlobalSafetyInsightsId, aggregate.TenantId, aggregate.Region.Code,
            primarySiteId, aggregate.TimeWindow, aggregate.Kpis, nowUtc, actor, correlation, causation);

        outboxWriter.Enqueue(kpiEvent, aggregate.TenantId.ToString(), "system", correlation.Value.ToString());

        foreach (var anomaly in aggregate.Anomalies)
        {
            var anomalyEvent = new GlobalSafetyAnomalyEvent(
                GlobalSafetyInsightsEventId.New(), aggregate.GlobalSafetyInsightsId, aggregate.TenantId, aggregate.Region.Code,
                anomaly.SiteId ?? primarySiteId, anomaly, nowUtc, actor, correlation, causation);

            outboxWriter.Enqueue(anomalyEvent, aggregate.TenantId.ToString(), "system", correlation.Value.ToString());
        }

        logger.LogInformation(
            "GlobalSafetyInsightsPublisher: enqueued 1 KPI snapshot event + {AnomalyCount} anomaly event(s) for snapshot {SnapshotId}.",
            aggregate.Anomalies.Count, aggregate.GlobalSafetyInsightsId);

        return Task.CompletedTask;
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights.Coordination
//  Layer:          Infrastructure
//  File:           CrossSiteActionEventPublisher.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §3 Multi-Site Coordination Workflows)
//  SchemaVersion:  2026.1
//  Description:    Real ICrossSiteActionEventPublisher implementation. Enforces data residency at
//                  the point of publication (mirroring GlobalSafetyInsightsPublisher's Phase 7
//                  IDataResidencyEnforcer check) before enqueuing the event onto the SAME real
//                  internal Outbox -> MassTransit pipeline every other domain event in this codebase
//                  already uses (IOutboxWriter.Enqueue, flushed by
//                  TransactionalUnitOfWork.CommitAsync / OutboxPipelineBehavior), rather than
//                  inventing a parallel publish mechanism. CrossSiteActionConsumer (already built,
//                  Phase 8 §4) is the real consumer on the other end. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.Application.GlobalSafetyInsights.Coordination.Publishers;
using PlantSafety.Domain.GlobalSafetyInsights.Events;

namespace PlantSafety.Infrastructure.GlobalSafetyInsights.Messaging.Publishers;

/// <summary>Publishes a <see cref="CrossSiteActionEvent"/> onto the real internal event mesh.</summary>
public sealed class CrossSiteActionEventPublisher(
    IOutboxWriter outboxWriter,
    IDataResidencyEnforcer residencyEnforcer,
    ILogger<CrossSiteActionEventPublisher> logger)
    : ICrossSiteActionEventPublisher
{
    /// <inheritdoc/>
    public Task PublishAsync(CrossSiteActionEvent evt, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(evt);

        // Phase 7 §5.3 Data Residency: reject/flag loudly rather than silently fanning out a
        // cross-site coordinated action outside the tenant's configured residency region.
        residencyEnforcer.EnsureSatisfied(evt.TenantId, evt.CorrelationId, evt.CausationId);

        outboxWriter.Enqueue(evt, evt.TenantId.ToString(), "system", evt.CorrelationId.Value.ToString());

        logger.LogInformation(
            "CrossSiteActionEventPublisher: enqueued CrossSiteActionEvent '{ActionType}' affecting {SiteCount} site(s) for Tenant {TenantId}.",
            evt.ActionType, evt.AffectedSiteIds.Count, evt.TenantId);

        return Task.CompletedTask;
    }
}

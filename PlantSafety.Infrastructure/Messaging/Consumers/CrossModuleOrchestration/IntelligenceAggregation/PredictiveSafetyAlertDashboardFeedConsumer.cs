// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation
//  Layer:          Infrastructure
//  File:           PredictiveSafetyAlertDashboardFeedConsumer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3.4 Intelligence <-> everything)
//  SchemaVersion:  2026.1
//  Description:    §3.4 aggregation example 2: every Intelligence.PredictiveSafety alert
//                  (regardless of which upstream module ultimately triggered it -- see
//                  InsufficientMitigationInsightConsumer and PredictiveMaintenanceEscalationConsumer
//                  for two concrete producers) is ingested into Intelligence.DataLake as a curated
//                  BI-facing record, which is the platform's real, existing ingestion surface
//                  consumed by Analytics.Dashboard / Analytics.ScoreCards / Dashboards.Widget --
//                  i.e. Intelligence "driving the new BI dashboards" per §3.4.
//                  Additive-only evolution.
//
//                  2026-08-02 UPDATE (Phase 6 replay-atomicity fix): CreateDataLakeActivityCommand
//                  always mints a new DataLakeActivity row (INSERT-only, no upsert key), so a
//                  redelivery must be rejected rather than allowed to feed a second duplicate BI
//                  record for the same alert. Chose the hard-reject dedup strategy accordingly (not
//                  an idempotent upsert), dedup'd by the alert's own PredictiveSafetyAlertId, wrapped
//                  in TryExecuteOnceAsync and scoped by a matching distributed lock.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Intelligence.DataLake.Commands;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.IntelligenceAggregation;

/// <summary>
/// Idempotent consumer that reacts to <see cref="PredictiveSafetyAlertCreatedEvent"/> by ingesting
/// a curated DataLake activity record for BI dashboard/widget consumption (Intelligence driving BI
/// dashboards).
/// </summary>
public sealed class PredictiveSafetyAlertDashboardFeedConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<PredictiveSafetyAlertDashboardFeedConsumer> logger)
    : CrossModuleIdempotentConsumerBase<PredictiveSafetyAlertCreatedEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    /// <inheritdoc/>
    protected override Guid ResolveEventId(PredictiveSafetyAlertCreatedEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<PredictiveSafetyAlertCreatedEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;

        // PredictiveSafetyAlertCreatedEvent already carries TenantId directly (unlike most other
        // domain events in this pass), so headers are only needed as a fallback.
        var tenantId = evt.TenantId != Guid.Empty ? evt.TenantId : context.GetEnvelopeTenantId();
        var actorId = evt.GeneratedBy.Value != Guid.Empty ? evt.GeneratedBy.Value : context.GetEnvelopeActorId();

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "PredictiveSafetyAlertDashboardFeedConsumer: no TenantId available for alert {AlertId}; skipping BI dashboard feed.",
                evt.PredictiveSafetyAlertId);
            return;
        }

        var alertId = evt.PredictiveSafetyAlertId.Value;

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:PredictiveSafetyAlertDashboardFeed:{alertId}",
            businessKey: alertId.ToString(),
            eventId: evt.EventId.Value,
            sideEffect: async () =>
            {
                var command = new CreateDataLakeActivityCommand(
                    CommandId: Guid.NewGuid(),
                    CorrelationId: evt.CorrelationId.Value,
                    CausationId: evt.EventId.Value,
                    RequestedAtUtc: DateTime.UtcNow,
                    ApiVersionToken: "v1",
                    TenantId: tenantId,
                    SiteId: evt.SiteId,
                    DatasetName: "Analytics.PredictiveSafetyAlertFeed",
                    Zone: "ML",
                    RecordCount: 1,
                    ProcessedByUserId: actorId);

                await DispatchAsync(command,
                    $"CreateDataLakeActivityCommand BI feed for predictive safety alert {evt.PredictiveSafetyAlertId} (category {evt.RiskCategory})",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}

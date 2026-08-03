// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.ProductionScheduling
//  Layer:          Infrastructure
//  File:           SchedulingKpiFeedbackConsumer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3.2 Production Tracking <-> Scheduling)
//  SchemaVersion:  2026.1
//  Description:    Direction 2 of §3.2: when a maintenance action is actually scheduled against a
//                  predictive alert (Operational.PredictiveMaintenance
//                  .PredictiveMaintenanceAlertActionScheduledEvent -- a real scheduling decision,
//                  since PredictiveScheduling itself has no persisted aggregate/event yet), compute
//                  the real production-KPI impact via the Section 2.3 ProductionKpiEngine (OEE,
//                  MTBF, MTTR, Throughput, Yield, Scrap Rate) using conservative, documented
//                  estimates for the scheduled maintenance window, then push the resulting snapshot
//                  into Intelligence.DataLake so Analytics.Dashboard / Analytics.ScoreCards /
//                  Dashboards.Widget can pick it up (§3.4 aggregation point). This demonstrates the
//                  real KPI-recompute path pending a dedicated "scheduling decision recorded"
//                  integration event and a persisted production-run data source (tracked
//                  follow-up).
//
//                  2026-08-02 UPDATE (Phase 6 replay-atomicity fix): CreateDataLakeActivityCommand
//                  always mints a brand-new DataLakeActivity aggregate row (INSERT-only, no upsert
//                  key on DatasetName/TenantId/etc -- see the handler and
//                  DataLakeActivityAggregateDapperRepository.AddAsync), so the recomputed KPI
//                  snapshot is NOT naturally idempotent: a redelivery would insert a second,
//                  duplicate curated KPI row rather than merely overwrite the first. Chose the
//                  hard-reject dedup strategy accordingly (not an idempotent upsert) -- wrapped in
//                  TryExecuteOnceAsync, deduplicated by the source PredictiveMaintenanceAlertActionScheduledEvent's
//                  own EventId, scoped by a matching distributed lock. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Analytics.Dashboard.Kpi;
using PlantSafety.Application.Intelligence.DataLake.Commands;
using PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.ProductionScheduling;

/// <summary>
/// Idempotent consumer that reacts to <see cref="PredictiveMaintenanceAlertActionScheduledEvent"/>
/// by recomputing the real production KPI suite via <see cref="ProductionKpiEngine"/> and feeding
/// the resulting snapshot into Intelligence.DataLake (§3.2 direction 2: scheduling changes update
/// production expectations/KPIs).
/// </summary>
public sealed class SchedulingKpiFeedbackConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<SchedulingKpiFeedbackConsumer> logger)
    : CrossModuleIdempotentConsumerBase<PredictiveMaintenanceAlertActionScheduledEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    private static readonly ProductionKpiEngine KpiEngine = new();

    // Conservative, documented defaults for the maintenance window's expected production impact
    // pending a real per-run telemetry data source (see file header). A scheduled corrective
    // action is modelled as an 8-hour planned window with a 2-hour downtime allowance for the
    // maintenance work itself.
    private static readonly TimeSpan PlannedWindow = TimeSpan.FromHours(8);
    private static readonly TimeSpan EstimatedDowntime = TimeSpan.FromHours(2);
    private static readonly TimeSpan IdealCycleTime = TimeSpan.FromMinutes(2);
    private const int EstimatedUnitsProducedInWindow = 180;
    private const int EstimatedGoodUnitsInWindow = 174;

    /// <inheritdoc/>
    protected override Guid ResolveEventId(PredictiveMaintenanceAlertActionScheduledEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<PredictiveMaintenanceAlertActionScheduledEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;
        var tenantId = context.GetEnvelopeTenantId();
        var actorId = context.GetEnvelopeActorId();

        var snapshot = KpiEngine.ComputeSnapshot(
            oeeInputs: new OeeInputs(PlannedWindow, EstimatedDowntime, IdealCycleTime, EstimatedUnitsProducedInWindow, EstimatedGoodUnitsInWindow),
            mtbfInputs: new MtbfInputs(PlannedWindow - EstimatedDowntime, failureCount: 1),
            mttrInputs: new MttrInputs(EstimatedDowntime, repairCount: 1),
            throughputInputs: new ThroughputInputs(EstimatedUnitsProducedInWindow, PlannedWindow),
            qualityInputs: new ProductionQualityInputs(
                EstimatedUnitsProducedInWindow, EstimatedGoodUnitsInWindow,
                EstimatedUnitsProducedInWindow - EstimatedGoodUnitsInWindow));

        Logger.LogInformation(
            "SchedulingKpiFeedbackConsumer: alert {AlertId} action scheduled -- recomputed OEE={Oee:P1} MTBF={Mtbf} MTTR={Mttr} Throughput={Throughput:F1}/hr Yield={Yield:P1} ScrapRate={ScrapRate:P1}.",
            evt.PredictiveMaintenanceAlertId, snapshot.Oee.Oee, snapshot.Mtbf, snapshot.Mttr,
            snapshot.ThroughputUnitsPerHour, snapshot.Yield, snapshot.ScrapRate);

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "SchedulingKpiFeedbackConsumer: no TenantId header on alert {AlertId}; skipping DataLake propagation.",
                evt.PredictiveMaintenanceAlertId);
            return;
        }

        var sourceEventId = evt.EventId.Value;

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:SchedulingKpiFeedback:{sourceEventId}",
            businessKey: sourceEventId.ToString(),
            eventId: sourceEventId,
            sideEffect: async () =>
            {
                var command = new CreateDataLakeActivityCommand(
                    CommandId: Guid.NewGuid(),
                    CorrelationId: evt.CorrelationId.Value,
                    CausationId: evt.EventId.Value,
                    RequestedAtUtc: DateTime.UtcNow,
                    ApiVersionToken: "v1",
                    TenantId: tenantId,
                    SiteId: null,
                    DatasetName: "Analytics.ProductionKpiRecompute",
                    Zone: "Curated",
                    RecordCount: 1,
                    ProcessedByUserId: actorId);

                await DispatchAsync(command,
                    $"CreateDataLakeActivityCommand for recomputed KPI snapshot (alert {evt.PredictiveMaintenanceAlertId})",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}

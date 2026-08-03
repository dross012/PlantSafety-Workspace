// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.ProductionScheduling
//  Layer:          Infrastructure
//  File:           ProductionAnomalyReprioritizationConsumer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3.2 Production Tracking <-> Scheduling)
//  SchemaVersion:  2026.1
//  Description:    Direction 1 of §3.2: routes real production/operational telemetry anomalies
//                  (Observability.MachineProcessMonitoring.MachineProcessMonitoringAnomalyFlaggedEvent
//                  -- the same category of signal ProductionTelemetryStreamProcessor's domain
//                  TelemetryAnomalyDetector produces, see PlantSafety.Application.Observability
//                  .Telemetry) into SchedulingReprioritizationOrchestrator
//                  .ReprioritizeForProductionStateChange so pending predictive-scheduling tasks for
//                  the affected machine are re-weighted to reflect the schedule slip. The
//                  PredictiveScheduling module has no persistence layer yet (see
//                  SchedulingReprioritizationOrchestrator's own header: "invoked by MediatR
//                  command/notification handlers ... performs no I/O itself" -- Phase 5 §1/2
//                  wave), so this consumer synthesizes a minimal SchedulableTask representing the
//                  affected machine's outstanding maintenance/production window, runs the real
//                  orchestrator, and records the reprioritization outcome as a DataLake activity
//                  for Intelligence/Analytics to pick up until a real scheduling task store exists
//                  (tracked follow-up once PMScheduling/WorkOrderPriority persistence lands).
//                  Additive-only evolution.
//
//                  2026-08-02 UPDATE (Phase 6 replay-atomicity fix): SchedulingReprioritizationOrchestrator
//                  performs no I/O of its own (see class header), so it is safe to recompute freely
//                  on every delivery; only the DataLake activity write below is a real, persisted
//                  side effect and is an INSERT-only creation (no natural upsert key), so it is
//                  wrapped in TryExecuteOnceAsync, deduplicated by the flagged anomaly's own
//                  MachineProcessMonitoringId (the anomaly occurrence's business identity -- distinct
//                  from the message envelope's own EventId, which IdempotentConsumerBase already
//                  covers), scoped by a matching distributed lock. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.SharedKernel.Abstractions.Concurrency;
using PlantSafety.Application.Intelligence.DataLake.Commands;
using PlantSafety.Application.Scheduling.PredictiveScheduling.Orchestration;
using PlantSafety.Domain.Observability.MachineProcessMonitoring.Events;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.ProductionScheduling;

/// <summary>
/// Idempotent consumer that reacts to <see cref="MachineProcessMonitoringAnomalyFlaggedEvent"/> by
/// invoking <see cref="SchedulingReprioritizationOrchestrator.ReprioritizeForProductionStateChange"/>
/// (§3.2 direction 1: production/operational state changes reprioritize pending schedule tasks).
/// </summary>
public sealed class ProductionAnomalyReprioritizationConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<ProductionAnomalyReprioritizationConsumer> logger)
    : CrossModuleIdempotentConsumerBase<MachineProcessMonitoringAnomalyFlaggedEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    private static readonly SchedulingReprioritizationOrchestrator Orchestrator = new(new PriorityWeightingService());

    /// <summary>Default synthetic task window used pending a real scheduling-task persistence layer.</summary>
    private static readonly TimeSpan DefaultTaskWindow = TimeSpan.FromHours(8);

    /// <inheritdoc/>
    protected override Guid ResolveEventId(MachineProcessMonitoringAnomalyFlaggedEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<MachineProcessMonitoringAnomalyFlaggedEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;
        var tenantId = context.GetEnvelopeTenantId();
        var actorId = context.GetEnvelopeActorId();
        var asOfUtc = evt.FlaggedAtUtc;

        var task = new SchedulableTask(
            taskId: SchedulableTaskId.New(),
            description: $"Production anomaly response: {evt.AnomalyDescription} ({evt.MachineProcessMonitoringId})",
            estimatedDuration: TimeSpan.FromHours(2),
            earliestStartUtc: asOfUtc,
            latestFinishUtc: new UtcTimestamp(asOfUtc.Value.Add(DefaultTaskWindow)),
            basePriorityWeight: 50.0);

        var outcome = Orchestrator.ReprioritizeForProductionStateChange(task, asOfUtc);

        Logger.LogInformation(
            "ProductionAnomalyReprioritizationConsumer: {Description} weight {Previous} -> {New} (immediate reoptimization required: {Immediate}).",
            task.Description, outcome.PreviousWeight, outcome.NewWeight, outcome.RequiresImmediateReoptimization);

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "ProductionAnomalyReprioritizationConsumer: no TenantId header on anomaly {AnomalyId}; skipping DataLake propagation.",
                evt.MachineProcessMonitoringId);
            return;
        }

        var anomalyId = evt.MachineProcessMonitoringId.Value;

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:ProductionAnomalyReprioritization:{anomalyId}",
            businessKey: anomalyId.ToString(),
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
                    SiteId: null,
                    DatasetName: "Scheduling.ProductionReprioritization",
                    Zone: "Curated",
                    RecordCount: 1,
                    ProcessedByUserId: actorId);

                await DispatchAsync(command,
                    $"CreateDataLakeActivityCommand for reprioritization outcome ({evt.MachineProcessMonitoringId})",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}

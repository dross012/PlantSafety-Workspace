// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.MaintenancePrediction
//  Layer:          Infrastructure
//  File:           MaintenanceCompletionFeedbackConsumer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3.1 Maintenance <-> Predictive Maintenance)
//  SchemaVersion:  2026.1
//  Description:    Direction 2 of §3.1: when a PreventiveMaintenanceTask is completed, feed the
//                  outcome back into Intelligence.DataLake as a curated training/feedback record
//                  (Intelligence.DataLake is the platform's real, existing ingestion surface for
//                  predictive-model inputs -- see §3.4) so FailureLikelihoodPredictor's next
//                  scoring pass for this equipment reflects that maintenance was actually
//                  performed (reducing RunHoursSinceLastPm / HistoricalFailureCount90Days inputs
//                  on the next model run). No FK from PreventiveMaintenanceTask back to the
//                  originating PredictiveMaintenanceAlert exists yet in the domain model (a
//                  genuine, documented gap -- see PredictiveMaintenanceEscalationConsumer, which
//                  propagates the alert's own CorrelationId onto the resulting task's
//                  CorrelationId so the two can still be correlated downstream via
//                  System.OutboxMessages.CorrelationId even without a direct foreign key).
//                  Additive-only evolution.
//
//                  2026-08-02 UPDATE (Phase 6 replay-atomicity fix): the DataLake activity write is
//                  an INSERT-only creation (CreateDataLakeActivityCommand always mints a new
//                  DataLakeActivity aggregate -- see its handler -- there is no natural upsert key),
//                  so a redelivery must be rejected rather than allowed to insert a second curated
//                  feedback record. Wrapped in TryExecuteOnceAsync, deduplicated by the completed
//                  task's own PreventiveMaintenanceTaskId, scoped by a matching distributed lock.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Intelligence.DataLake.Commands;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.MaintenancePrediction;

/// <summary>
/// Idempotent consumer that reacts to <see cref="PreventiveMaintenanceTaskCompletedEvent"/> by
/// ingesting a DataLake activity record so Intelligence's predictive models see the completed
/// maintenance as a fresh input signal (§3.1 direction 2 feedback loop).
/// </summary>
public sealed class MaintenanceCompletionFeedbackConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<MaintenanceCompletionFeedbackConsumer> logger)
    : CrossModuleIdempotentConsumerBase<PreventiveMaintenanceTaskCompletedEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    private const string DatasetName = "PredictiveMaintenance.CompletedTaskFeedback";
    private const string Zone = "Curated";

    /// <inheritdoc/>
    protected override Guid ResolveEventId(PreventiveMaintenanceTaskCompletedEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<PreventiveMaintenanceTaskCompletedEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;
        var tenantId = context.GetEnvelopeTenantId();
        var actorId = context.GetEnvelopeActorId();

        if (tenantId == Guid.Empty)
        {
            Logger.LogWarning(
                "MaintenanceCompletionFeedbackConsumer: no TenantId header on task {TaskId}; skipping predictive-model feedback ingestion.",
                evt.PreventiveMaintenanceTaskId);
            return;
        }

        var taskId = evt.PreventiveMaintenanceTaskId.Value;

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:MaintenanceCompletionFeedback:{taskId}",
            businessKey: taskId.ToString(),
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
                    DatasetName: DatasetName,
                    Zone: Zone,
                    RecordCount: 1,
                    ProcessedByUserId: actorId);

                await DispatchAsync(command,
                    $"CreateDataLakeActivityCommand feedback for completed task {evt.PreventiveMaintenanceTaskId}",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}

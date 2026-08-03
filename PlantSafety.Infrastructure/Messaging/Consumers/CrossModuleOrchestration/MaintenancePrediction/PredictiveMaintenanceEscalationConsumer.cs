// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Messaging.Consumers.CrossModuleOrchestration.MaintenancePrediction
//  Layer:          Infrastructure
//  File:           PredictiveMaintenanceEscalationConsumer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3.1 Maintenance <-> Predictive Maintenance)
//  SchemaVersion:  2026.1
//  Description:    Direction 1 of §3.1: when a predictive-failure alert is raised for a piece of
//                  equipment (Operational.PredictiveMaintenance.PredictiveMaintenanceAlertRaisedEvent
//                  -- the real event already raised once FailureLikelihoodPredictor.ShouldRaiseAlert
//                  is true, see PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors
//                  .FailureLikelihoodPredictor.AlertThreshold), automatically schedule a real
//                  PreventiveMaintenanceTask via the existing
//                  SchedulePreventiveMaintenanceTaskCommand so the predicted failure is actually
//                  actioned instead of only sitting as an unread alert.
//
//                  2026-08-02 UPDATE (Phase 6 replay-atomicity fix): the task-scheduling command
//                  dispatch is wrapped in TryExecuteOnceAsync, deduplicated by the triggering
//                  alert's own PredictiveMaintenanceAlertId (a redelivery of the same alert must
//                  never schedule a second PreventiveMaintenanceTask), scoped by a distributed lock
//                  keyed identically so two concurrent redeliveries serialize. Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Operational.PreventiveMaintenance.Commands;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Events;
using PlantSafety.SharedKernel.Abstractions.Concurrency;

namespace PlantSafety.Infrastructure.Messaging.Consumers.CrossModuleOrchestration.MaintenancePrediction;

/// <summary>
/// Idempotent consumer that reacts to <see cref="PredictiveMaintenanceAlertRaisedEvent"/> by
/// scheduling a corresponding <c>PreventiveMaintenanceTask</c> (Operational.PredictiveMaintenance
/// -&gt; Operational.PreventiveMaintenance cross-module wiring, §3.1 direction 1).
/// </summary>
public sealed class PredictiveMaintenanceEscalationConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IDistributedLockProvider lockProvider,
    ICrossModuleDedupStore dedupStore,
    ILogger<PredictiveMaintenanceEscalationConsumer> logger)
    : CrossModuleIdempotentConsumerBase<PredictiveMaintenanceAlertRaisedEvent>(connectionFactory, sender, lockProvider, dedupStore, logger)
{
    /// <summary>The lead time given to the newly-scheduled preventive task ahead of the predicted failure.</summary>
    private static readonly TimeSpan SchedulingLeadTime = TimeSpan.FromDays(1);

    /// <inheritdoc/>
    protected override Guid ResolveEventId(PredictiveMaintenanceAlertRaisedEvent message) => message.EventId.Value;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<PredictiveMaintenanceAlertRaisedEvent> context,
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
                "PredictiveMaintenanceEscalationConsumer: no TenantId header on alert {AlertId}; skipping cross-module task scheduling.",
                evt.PredictiveMaintenanceAlertId);
            return;
        }

        var alertId = evt.PredictiveMaintenanceAlertId.Value;

        await TryExecuteOnceAsync(
            lockResourceKey: $"CrossModule:MaintenanceEscalation:{alertId}",
            businessKey: alertId.ToString(),
            eventId: evt.EventId.Value,
            sideEffect: async () =>
            {
                var command = SchedulePreventiveMaintenanceTaskCommand.FromDto(
                    tenantId: tenantId,
                    siteId: null,
                    equipmentId: evt.EquipmentId,
                    taskDescription: $"Predictive maintenance follow-up: {evt.PredictedFailureType} (alert {evt.PredictiveMaintenanceAlertId})",
                    scheduledAtUtc: evt.PredictedAtUtc.Value.Add(SchedulingLeadTime),
                    requestedByUserId: actorId,
                    correlationId: evt.CorrelationId.Value,
                    causationId: evt.EventId.Value);

                await DispatchAsync(command,
                    $"SchedulePreventiveMaintenanceTaskCommand for equipment {evt.EquipmentId} (alert {evt.PredictiveMaintenanceAlertId})",
                    cancellationToken);
            },
            cancellationToken: cancellationToken);
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh
//  Layer:          Infrastructure
//  File:           WorkOrderCompletedEnterpriseMeshConsumer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Real, working outbound Enterprise Event Mesh wiring point: when a WorkOrder is
//                  completed (published onto the bus by OutboxProcessor from
//                  [System].[OutboxMessages], exactly like every other cross-module consumer in this
//                  codebase), maps the event to an EnterpriseEventEnvelope and "sends" it to every
//                  registered IExternalSystemPublisher (currently ERP and MES; each gracefully
//                  no-ops if its endpoint is unconfigured -- see HttpExternalSystemPublisher).
//                  Idempotent via IdempotentConsumerBase (System.ProcessedEvents keyed by EventId),
//                  matching every other consumer's hardening convention. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.Events;
using PlantSafety.Infrastructure.Messaging.Consumers;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh;

/// <summary>Idempotent consumer that propagates <see cref="WorkOrderCompletedEvent"/> to the Enterprise Event Mesh's external publishers.</summary>
public sealed class WorkOrderCompletedEnterpriseMeshConsumer(
    ISqlConnectionFactory connectionFactory,
    IEnumerable<IExternalSystemPublisher> publishers,
    ILogger<WorkOrderCompletedEnterpriseMeshConsumer> logger)
    : IdempotentConsumerBase<WorkOrderCompletedEvent>(connectionFactory, logger)
{
    /// <inheritdoc/>
    protected override Guid ResolveEventId(WorkOrderCompletedEvent message) => message.EventId;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<WorkOrderCompletedEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;
        var tenantId = context.GetEnvelopeTenantId();
        var actorId = context.GetEnvelopeActorId();

        var envelope = WorkOrderEnterpriseEventMapper.ToEnvelope(evt, tenantId, actorId);

        foreach (var publisher in publishers)
        {
            var result = await publisher.PublishAsync(envelope, cancellationToken);

            if (result.IsFailure)
            {
                // Logged by the publisher itself; a genuine outbound transport failure to one
                // external system must not block propagation to the others, and must not fault
                // this consumer (which would trigger MassTransit redelivery of an event this
                // consumer's own idempotency ledger would then skip on retry -- the failed
                // publish would never be retried). Recorded here at Warning for operator
                // visibility; re-drive tooling for failed enterprise-mesh publishes is out of
                // scope for this pass, matching Shared.EventBus's own "dead-letter is manual
                // re-drive" precedent.
                logger.LogWarning(
                    "EnterpriseEventMesh: WorkOrder {WorkOrderId} completion propagation to {TargetSystem} failed: {Message}",
                    evt.WorkOrderId, publisher.TargetSystem, result.Error!.Message);
            }
        }
    }
}

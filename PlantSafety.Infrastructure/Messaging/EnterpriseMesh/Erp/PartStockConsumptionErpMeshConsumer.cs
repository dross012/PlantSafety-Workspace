// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh.Erp
//  Layer:          Infrastructure
//  File:           PartStockConsumptionErpMeshConsumer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Real, working outbound Enterprise Event Mesh wiring point for ERP inventory/parts
//                  consumption sync: when a Part's on-hand quantity is adjusted (the real Operational
//                  .WorkOrders Part aggregate -- restock or work-order consumption), maps the event
//                  to an EnterpriseEventEnvelope and publishes it to every registered ERP-targeted
//                  IExternalSystemPublisher (gracefully no-ops if the ERP endpoint is unconfigured).
//                  Idempotent via IdempotentConsumerBase, matching every other consumer's hardening
//                  convention. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.WorkOrders.Events;
using PlantSafety.Domain.Operational.WorkOrders.Interfaces;
using PlantSafety.Infrastructure.Messaging.Consumers;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh.Erp;

/// <summary>Idempotent consumer that propagates <see cref="PartStockAdjustedEvent"/> to the ERP-targeted publishers on the Enterprise Event Mesh.</summary>
public sealed class PartStockConsumptionErpMeshConsumer(
    ISqlConnectionFactory connectionFactory,
    IPartRepository partRepository,
    IEnumerable<IExternalSystemPublisher> publishers,
    ILogger<PartStockConsumptionErpMeshConsumer> logger)
    : IdempotentConsumerBase<PartStockAdjustedEvent>(connectionFactory, logger)
{
    /// <inheritdoc/>
    protected override Guid ResolveEventId(PartStockAdjustedEvent message) => message.EventId;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<PartStockAdjustedEvent> context,
        IDbConnection connection,
        IDbTransaction transaction,
        CancellationToken cancellationToken)
    {
        var evt = context.Message;
        var tenantId = context.GetEnvelopeTenantId();
        var actorId = context.GetEnvelopeActorId();

        if (tenantId == Guid.Empty)
        {
            logger.LogWarning(
                "EnterpriseEventMesh: no TenantId header on Part {PartId} stock adjustment; skipping ERP propagation.",
                evt.PartId.Value);
            return;
        }

        var part = await partRepository.GetByIdAsync(evt.PartId, tenantId, cancellationToken);
        if (part is null)
        {
            logger.LogWarning(
                "EnterpriseEventMesh: Part {PartId} not found for Tenant {TenantId}; skipping ERP propagation.",
                evt.PartId.Value, tenantId);
            return;
        }

        var envelope = PartStockConsumptionEnterpriseEventMapper.ToEnvelope(evt, part, tenantId, actorId);

        foreach (var publisher in publishers.Where(p => p.TargetSystem == EnterpriseSourceSystem.Erp))
        {
            var result = await publisher.PublishAsync(envelope, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "EnterpriseEventMesh: Part {PartId} stock-adjustment propagation to {TargetSystem} failed: {Message}",
                    evt.PartId.Value, publisher.TargetSystem, result.Error!.Message);
            }
        }
    }
}

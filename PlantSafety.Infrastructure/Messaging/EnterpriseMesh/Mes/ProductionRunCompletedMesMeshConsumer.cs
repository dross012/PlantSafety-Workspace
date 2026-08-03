// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh.Mes
//  Layer:          Infrastructure
//  File:           ProductionRunCompletedMesMeshConsumer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.2 Enterprise System Integrations, MES)
//  SchemaVersion:  2026.1
//  Description:    Real, working outbound Enterprise Event Mesh wiring point for MES production-order
//                  sync: when a ProductionRun completes, rehydrates the aggregate (for the real
//                  ProductionRunToCanonicalMapper's full V2 shape) and publishes the resulting
//                  canonical production-order envelope to every registered MES- and ERP-targeted
//                  IExternalSystemPublisher. Idempotent via IdempotentConsumerBase. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Events;
using PlantSafety.Domain.Operational.ProductionTracking.Interfaces;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;
using PlantSafety.Infrastructure.Messaging.Consumers;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh.Mes;

/// <summary>Idempotent consumer that propagates <see cref="ProductionRunCompletedEvent"/> to the MES/ERP-targeted publishers on the Enterprise Event Mesh.</summary>
public sealed class ProductionRunCompletedMesMeshConsumer(
    ISqlConnectionFactory connectionFactory,
    IProductionRunRepository productionRunRepository,
    IEnumerable<IExternalSystemPublisher> publishers,
    ProductionOrderEnterpriseEventMapper mapper,
    ILogger<ProductionRunCompletedMesMeshConsumer> logger)
    : IdempotentConsumerBase<ProductionRunCompletedEvent>(connectionFactory, logger)
{
    /// <inheritdoc/>
    protected override Guid ResolveEventId(ProductionRunCompletedEvent message) => message.EventId;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<ProductionRunCompletedEvent> context,
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
                "EnterpriseEventMesh: no TenantId header on ProductionRun {ProductionRunId} completion; skipping MES/ERP propagation.",
                evt.ProductionRunId.Value);
            return;
        }

        var run = await productionRunRepository.GetByIdAsync(ProductionRunId.FromGuid(evt.ProductionRunId.Value), tenantId, cancellationToken);
        if (run is null)
        {
            logger.LogWarning(
                "EnterpriseEventMesh: ProductionRun {ProductionRunId} not found for Tenant {TenantId}; skipping MES/ERP propagation.",
                evt.ProductionRunId.Value, tenantId);
            return;
        }

        var envelope = mapper.ToEnvelope(run, evt, tenantId, actorId);

        foreach (var publisher in publishers.Where(p => p.TargetSystem is EnterpriseSourceSystem.Mes or EnterpriseSourceSystem.Erp))
        {
            var result = await publisher.PublishAsync(envelope, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "EnterpriseEventMesh: ProductionRun {ProductionRunId} completion propagation to {TargetSystem} failed: {Message}",
                    evt.ProductionRunId.Value, publisher.TargetSystem, result.Error!.Message);
            }
        }
    }
}

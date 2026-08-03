// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh.Erp
//  Layer:          Infrastructure
//  File:           FinancialRecordApprovedErpMeshConsumer.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Real, working outbound Enterprise Event Mesh wiring point for ERP financial
//                  posting: when a FinancialRecord is approved (the real Financials module's genuine
//                  financial-impact event -- see FinancialRecordApprovedEnterpriseEventMapper's
//                  header for why this, not a fabricated "work order with cost" scenario), enriches
//                  the event with the record's Amount/Category (via the real
//                  FinancialRecordByIdQuery) and, when configured, its ERP cost center code (via the
//                  real GetCostCenterMappingQuery), then publishes to every registered ERP-targeted
//                  IExternalSystemPublisher. Idempotent via IdempotentConsumerBase. Additive-only
//                  evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Enterprise.Integrations.Erp.CostCenters;
using PlantSafety.Application.Financials.Queries;
using PlantSafety.Domain.Financials.Events;
using PlantSafety.Infrastructure.Messaging.Consumers;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh.Erp;

/// <summary>Idempotent consumer that propagates <see cref="FinancialRecordApprovedEvent"/> to the ERP-targeted publishers on the Enterprise Event Mesh.</summary>
public sealed class FinancialRecordApprovedErpMeshConsumer(
    ISqlConnectionFactory connectionFactory,
    ISender sender,
    IEnumerable<IExternalSystemPublisher> publishers,
    ILogger<FinancialRecordApprovedErpMeshConsumer> logger)
    : IdempotentConsumerBase<FinancialRecordApprovedEvent>(connectionFactory, logger)
{
    /// <inheritdoc/>
    protected override Guid ResolveEventId(FinancialRecordApprovedEvent message) => message.EventId;

    /// <inheritdoc/>
    protected override async Task HandleAsync(
        ConsumeContext<FinancialRecordApprovedEvent> context,
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
                "EnterpriseEventMesh: no TenantId header on FinancialRecord {FinancialRecordId} approval; skipping ERP posting.",
                evt.FinancialRecordId);
            return;
        }

        var recordResult = await sender.Send(new FinancialRecordByIdQuery(tenantId, evt.FinancialRecordId), cancellationToken);
        if (recordResult.IsFailure)
        {
            logger.LogWarning(
                "EnterpriseEventMesh: could not load FinancialRecord {FinancialRecordId} for ERP posting: [{ErrorCode}] {Message}",
                evt.FinancialRecordId, recordResult.Error.ErrorCode, recordResult.Error.Message);
            return;
        }

        var record = recordResult.Value;

        var costCenterResult = await sender.Send(new GetCostCenterMappingQuery(tenantId, record.SiteId), cancellationToken);
        var costCenterMapping = costCenterResult.IsSuccess ? costCenterResult.Value : null;

        var envelope = FinancialRecordApprovedEnterpriseEventMapper.ToEnvelope(evt, record, costCenterMapping, tenantId, actorId);

        foreach (var publisher in publishers.Where(p => p.TargetSystem == EnterpriseSourceSystem.Erp))
        {
            var result = await publisher.PublishAsync(envelope, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogWarning(
                    "EnterpriseEventMesh: FinancialRecord {FinancialRecordId} approval-posting propagation to {TargetSystem} failed: {Message}",
                    evt.FinancialRecordId, publisher.TargetSystem, result.Error!.Message);
            }
        }
    }
}

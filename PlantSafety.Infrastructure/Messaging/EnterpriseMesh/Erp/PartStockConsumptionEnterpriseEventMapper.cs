// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh.Erp
//  Layer:          Infrastructure
//  File:           PartStockConsumptionEnterpriseEventMapper.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    Maps the real, internal PartStockAdjustedEvent onto the canonical cross-system
//                  EnterpriseEventEnvelope for outbound propagation to ERP (inventory/parts
//                  consumption sync). EventType follows the namespaced, versioned convention:
//                  "plantsafety.part.stockadjusted.v1". Additive-only evolution.
//
//                  2026-08-03 UPDATE (Phase 7 §1.3 Tenant-Aware Eventing): converted from a static
//                  class to an instance class carrying an injected ITenantRegionRouter, so the
//                  envelope's Region field can be populated by resolving the event's TenantId
//                  through the same tenant-region-affinity logic Phase 7 §4.3 Global Routing already
//                  established, instead of always defaulting to null. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Enterprise.MultiRegion;
using PlantSafety.Domain.Operational.WorkOrders.Aggregates;
using PlantSafety.Domain.Operational.WorkOrders.Events;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh.Erp;

/// <summary>Maps Part domain events onto <see cref="EnterpriseEventEnvelope"/> for the Enterprise Event Mesh.</summary>
public sealed class PartStockConsumptionEnterpriseEventMapper(ITenantRegionRouter tenantRegionRouter)
{
    /// <summary>The canonical, namespaced EventType for this envelope.</summary>
    public const string StockAdjustedEventType = "plantsafety.part.stockadjusted.v1";

    /// <summary>The current schema version this mapper produces.</summary>
    public const int CurrentSchemaVersion = 1;

    /// <summary>
    /// Maps a <see cref="PartStockAdjustedEvent"/>, enriched with the owning <paramref name="part"/>
    /// record for part-number/location context ERP inventory systems require, onto an outbound
    /// <see cref="EnterpriseEventEnvelope"/>.
    /// </summary>
    public EnterpriseEventEnvelope ToEnvelope(PartStockAdjustedEvent domainEvent, Part part, Guid tenantId, Guid actorId) =>
        EnterpriseEventEnvelope.Create(
            eventType: StockAdjustedEventType,
            schemaVersion: CurrentSchemaVersion,
            sourceSystem: EnterpriseSourceSystem.Platform,
            occurredAtUtc: domainEvent.AdjustedAtUtc.Value,
            correlationId: domainEvent.CorrelationId.Value,
            causationId: domainEvent.EventId,
            tenantId: tenantId,
            actorId: actorId == Guid.Empty ? null : actorId,
            payload: new PartStockConsumptionEnterprisePayload(
                domainEvent.PartId.Value,
                part.PartNumber,
                part.PartName,
                domainEvent.DeltaQuantity,
                domainEvent.NewQuantityOnHand,
                part.Location,
                domainEvent.Reason,
                domainEvent.AdjustedAtUtc.Value),
            region: tenantId == Guid.Empty ? null : tenantRegionRouter.ResolveRegion(tenantId).Code);

    /// <summary>Payload shape for <see cref="StockAdjustedEventType"/> SchemaVersion 1.</summary>
    /// <param name="PartId">Identity of the adjusted Part.</param>
    /// <param name="PartNumber">Human-readable part number.</param>
    /// <param name="PartName">Part display name.</param>
    /// <param name="DeltaQuantity">Signed quantity change (negative = consumption).</param>
    /// <param name="NewQuantityOnHand">Quantity on hand after the adjustment.</param>
    /// <param name="Location">Storage location.</param>
    /// <param name="Reason">Free-text adjustment reason, if any.</param>
    /// <param name="AdjustedAtUtc">UTC timestamp of the adjustment.</param>
    public sealed record PartStockConsumptionEnterprisePayload(
        Guid PartId,
        string PartNumber,
        string PartName,
        int DeltaQuantity,
        int NewQuantityOnHand,
        string Location,
        string? Reason,
        DateTime AdjustedAtUtc);
}

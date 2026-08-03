// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh.Mes
//  Layer:          Infrastructure
//  File:           ProductionOrderEnterpriseEventMapper.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.2 Enterprise System Integrations, MES)
//  SchemaVersion:  2026.1
//  Description:    Maps the real ProductionRun aggregate (via the real ProductionRunToCanonicalMapper,
//                  never a re-implemented mapping) onto the canonical cross-system
//                  EnterpriseEventEnvelope for outbound propagation to MES/ERP on ProductionRun
//                  completion. EventType: "plantsafety.productionorder.completed.v1". Additive-only
//                  evolution.
//
//                  2026-08-03 UPDATE (Phase 7 §1.3 Tenant-Aware Eventing): converted from a static
//                  class to an instance class carrying an injected ITenantRegionRouter, so the
//                  envelope's Region field can be populated by resolving the event's TenantId
//                  through the same tenant-region-affinity logic Phase 7 §4.3 Global Routing already
//                  established, instead of always defaulting to null. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Enterprise.MultiRegion;
using PlantSafety.Application.Operational.ProductionTracking.Mappings;
using PlantSafety.Domain.Operational.ProductionTracking.Aggregates;
using PlantSafety.Domain.Operational.ProductionTracking.Events;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh.Mes;

/// <summary>Maps a completed <see cref="ProductionRun"/> onto <see cref="EnterpriseEventEnvelope"/> for the Enterprise Event Mesh.</summary>
public sealed class ProductionOrderEnterpriseEventMapper(ITenantRegionRouter tenantRegionRouter)
{
    /// <summary>The canonical, namespaced EventType for this envelope.</summary>
    public const string CompletedEventType = "plantsafety.productionorder.completed.v1";

    /// <summary>Maps a completed <paramref name="run"/> and the raising <paramref name="domainEvent"/> onto an outbound <see cref="EnterpriseEventEnvelope"/>.</summary>
    public EnterpriseEventEnvelope ToEnvelope(ProductionRun run, ProductionRunCompletedEvent domainEvent, Guid tenantId, Guid actorId) =>
        EnterpriseEventEnvelope.Create(
            eventType: CompletedEventType,
            schemaVersion: CanonicalProductionOrderV2.SchemaVersion,
            sourceSystem: EnterpriseSourceSystem.Platform,
            occurredAtUtc: domainEvent.EndedAtUtc.Value,
            correlationId: domainEvent.CorrelationId.Value,
            causationId: domainEvent.EventId,
            tenantId: tenantId,
            actorId: actorId == Guid.Empty ? null : actorId,
            payload: run.ToCanonicalV2(),
            region: tenantId == Guid.Empty ? null : tenantRegionRouter.ResolveRegion(tenantId).Code);
}

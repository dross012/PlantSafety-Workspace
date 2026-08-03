// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh
//  Layer:          Infrastructure
//  File:           WorkOrderEnterpriseEventMapper.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.1/§3.2 Enterprise Event Mesh, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Maps the real, internal WorkOrderCompletedEvent onto the canonical cross-system
//                  EnterpriseEventEnvelope for outbound propagation to ERP/MES. EventType follows the
//                  namespaced, versioned convention this Phase 6 §3.1 section specifies:
//                  "plantsafety.workorder.completed.v1". Additive-only evolution.
//
//                  2026-08-03 UPDATE (Phase 7 §1.3 Tenant-Aware Eventing): converted from a static
//                  class to an instance class carrying an injected ITenantRegionRouter, so the
//                  envelope's Region field can be populated by resolving the event's TenantId
//                  through the same tenant-region-affinity logic Phase 7 §4.3 Global Routing already
//                  established, instead of always defaulting to null. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Enterprise.MultiRegion;
using PlantSafety.Domain.Operational.WorkOrders.Events;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh;

/// <summary>Maps WorkOrder domain events onto <see cref="EnterpriseEventEnvelope"/> for the Enterprise Event Mesh.</summary>
public sealed class WorkOrderEnterpriseEventMapper(ITenantRegionRouter tenantRegionRouter)
{
    /// <summary>The canonical, namespaced EventType for this envelope.</summary>
    public const string CompletedEventType = "plantsafety.workorder.completed.v1";

    /// <summary>The current schema version this mapper produces.</summary>
    public const int CurrentSchemaVersion = 1;

    /// <summary>Maps a <see cref="WorkOrderCompletedEvent"/> and its propagated outbox context onto an outbound <see cref="EnterpriseEventEnvelope"/>.</summary>
    public EnterpriseEventEnvelope ToEnvelope(WorkOrderCompletedEvent domainEvent, Guid tenantId, Guid actorId) =>
        EnterpriseEventEnvelope.Create(
            eventType: CompletedEventType,
            schemaVersion: CurrentSchemaVersion,
            sourceSystem: EnterpriseSourceSystem.Platform,
            occurredAtUtc: domainEvent.CompletedAtUtc.Value,
            correlationId: domainEvent.CorrelationId.Value,
            causationId: domainEvent.EventId,
            tenantId: tenantId,
            actorId: actorId == Guid.Empty ? null : actorId,
            payload: new WorkOrderCompletedEnterprisePayload(
                domainEvent.WorkOrderId.Value,
                domainEvent.CompletionNotes,
                domainEvent.CompletedAtUtc.Value),
            region: tenantId == Guid.Empty ? null : tenantRegionRouter.ResolveRegion(tenantId).Code);

    /// <summary>Payload shape for <see cref="CompletedEventType"/> SchemaVersion 1.</summary>
    /// <param name="WorkOrderId">Identity of the completed WorkOrder.</param>
    /// <param name="CompletionNotes">Free-text completion notes, if any.</param>
    /// <param name="CompletedAtUtc">UTC timestamp the WorkOrder was completed.</param>
    public sealed record WorkOrderCompletedEnterprisePayload(Guid WorkOrderId, string? CompletionNotes, DateTime CompletedAtUtc);
}

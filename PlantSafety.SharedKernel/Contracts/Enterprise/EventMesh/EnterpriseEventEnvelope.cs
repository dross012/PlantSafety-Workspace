// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts.Enterprise.EventMesh
//  Layer:          SharedKernel
//  File:           EnterpriseEventEnvelope.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3 Enterprise Event Mesh, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Canonical, versioned envelope for CROSS-SYSTEM (not just cross-module) event
//                  exchange between this platform and external Enterprise Resource Planning (ERP),
//                  Manufacturing Execution System (MES), and Computerized Maintenance Management
//                  System (CMMS) integrations -- distinct from (but able to wrap/carry) the
//                  platform's existing internal <c>[System].[OutboxMessages]</c> envelope (see
//                  PlantSafety.Infrastructure.Messaging.OutboxEnvelope) and the existing SQL-queue
//                  cross-MODULE bus (Shared.EventBus.Infrastructure; see that project's README for
//                  why this is a distinct concern -- it explicitly does not cover cross-SYSTEM
//                  propagation to external ERP/MES/CMMS endpoints).
//
//                  Follows the Handbook's Canonical Event Envelope Schema (Part XIV §14.9) fields
//                  (EventId, EventType, CorrelationId, CausationId, TimestampUtc, ActorId, TenantId,
//                  Payload) plus the cross-system-specific fields this Phase 6 §3 section requires
//                  (SchemaVersion, SourceSystem). <c>PlantSafety.SharedKernel.Contracts.Enterprise
//                  .CanonicalModel</c> (a concurrent agent's canonical-DTO work) had not landed at
//                  the time this file was written, so <see cref="Payload"/> is generic over
//                  <see cref="JsonElement"/> rather than a canonical DTO union -- when that work
//                  lands, a typed accessor (e.g. <c>Payload.Deserialize&lt;TCanonicalDto&gt;()</c>)
//                  can be layered on top of this envelope without any breaking change to the shape
//                  below (additive-only evolution).
// ============================================================================
using System.Text.Json;

namespace PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

/// <summary>
/// Canonical, versioned envelope for cross-system (platform &lt;-&gt; external ERP/MES/CMMS) event
/// exchange. Immutable; every event propagated across the Enterprise Event Mesh -- inbound or
/// outbound -- is carried in one of these envelopes.
/// </summary>
/// <param name="EventId">Stable, globally unique identifier for this specific event occurrence (UUIDv7 per Handbook §5.5/14.9).</param>
/// <param name="EventType">
/// Stable, namespaced event-type discriminator, e.g. <c>"plantsafety.workorder.completed.v1"</c> for
/// platform-originated events or <c>"erp.workorder.statuschanged.v1"</c> for an external-system-originated
/// event. The trailing <c>.v{N}</c> suffix is a human-readable convenience only; <see cref="SchemaVersion"/>
/// is the authoritative version discriminator used by the upgrade pipeline (see <see cref="IEventUpgrader"/>).
/// </param>
/// <param name="SchemaVersion">
/// The authoritative schema version of <see cref="Payload"/> for this <see cref="EventType"/>, starting at 1
/// and incrementing only on additive change (Handbook Part XVI). Never decreases; never reused for an
/// incompatible shape.
/// </param>
/// <param name="SourceSystem">The system that originated this event.</param>
/// <param name="OccurredAtUtc">UTC timestamp the event actually occurred at its origin (not when it was received/relayed).</param>
/// <param name="CorrelationId">Correlates this event with the broader business operation/saga it belongs to.</param>
/// <param name="CausationId">The identifier of the event/command that directly caused this event, if any.</param>
/// <param name="TenantId">The tenant this event belongs to.</param>
/// <param name="ActorId">The identifier of the actor (user/system/workflow) that caused this event at its origin, if known.</param>
/// <param name="Payload">
/// The event-specific payload, generic over <see cref="JsonElement"/> until the canonical
/// <c>PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel</c> DTOs land (see file header).
/// </param>
public sealed record EnterpriseEventEnvelope(
    Guid EventId,
    string EventType,
    int SchemaVersion,
    EnterpriseSourceSystem SourceSystem,
    DateTime OccurredAtUtc,
    Guid CorrelationId,
    Guid? CausationId,
    Guid TenantId,
    Guid? ActorId,
    JsonElement Payload)
{
    /// <summary>
    /// Creates a new outbound <see cref="EnterpriseEventEnvelope"/> with a freshly minted
    /// <see cref="EventId"/>, serializing <paramref name="payload"/> to a <see cref="JsonElement"/>.
    /// </summary>
    public static EnterpriseEventEnvelope Create<TPayload>(
        string eventType,
        int schemaVersion,
        EnterpriseSourceSystem sourceSystem,
        DateTime occurredAtUtc,
        Guid correlationId,
        Guid? causationId,
        Guid tenantId,
        Guid? actorId,
        TPayload payload)
    {
        var payloadElement = JsonSerializer.SerializeToElement(payload);

        return new EnterpriseEventEnvelope(
            EventId: Guid.NewGuid(),
            EventType: eventType,
            SchemaVersion: schemaVersion,
            SourceSystem: sourceSystem,
            OccurredAtUtc: occurredAtUtc,
            CorrelationId: correlationId,
            CausationId: causationId,
            TenantId: tenantId,
            ActorId: actorId,
            Payload: payloadElement);
    }

    /// <summary>Deserializes <see cref="Payload"/> into <typeparamref name="TPayload"/>.</summary>
    public TPayload? DeserializePayload<TPayload>() => Payload.Deserialize<TPayload>();
}

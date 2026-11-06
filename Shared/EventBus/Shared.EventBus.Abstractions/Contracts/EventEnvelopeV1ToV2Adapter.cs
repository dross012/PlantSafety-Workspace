// ============================================================================
//  Project:        Shared.EventBus.Abstractions
//  File:           EventEnvelopeV1ToV2Adapter.cs
//  Created:        2026-08-08
//  Description:    Phase B.4 migration scaffolding (TEMPORARY): structurally
//                  upgrades a legacy EventEnvelope (V1) into an EventEnvelopeV2
//                  with every enrichment field null (V1 carries none of that
//                  data, so there is nothing to fill in without inventing
//                  it). Exists so a V2 subscriber can optionally consume a
//                  V1-only event stream during the coexistence window,
//                  rather than requiring every publisher to migrate before
//                  any V2 subscriber can run. Delete once every publisher
//                  this app cares about publishes true V2 envelopes with
//                  real enrichment.
// ============================================================================
namespace Shared.EventBus.Abstractions.Contracts;

/// <summary>Upgrades a V1 <see cref="EventEnvelope"/> into a V2 <see cref="EventEnvelopeV2"/> with null enrichment fields.</summary>
public static class EventEnvelopeV1ToV2Adapter
{
    /// <summary>Structurally upgrades <paramref name="v1"/>. Every enrichment field (FacilityId/IndustryName/Location/HazardType) is null.</summary>
    public static EventEnvelopeV2 Upgrade(EventEnvelope v1) => new()
    {
        EventId = v1.EventId,
        EventType = v1.EventType,
        SchemaVersion = v1.SchemaVersion,
        PayloadType = v1.PayloadType,
        TenantId = v1.TenantId,
        CorrelationId = v1.CorrelationId,
        CausationId = v1.CausationId,
        TimestampUtc = v1.OccurredAtUtc,
        SourceModule = v1.SourceModule,
        Payload = v1.Payload,
        EnqueuedAtUtc = v1.EnqueuedAtUtc,
        FacilityId = null,
        IndustryName = null,
        Location = null,
        HazardType = null,
    };
}

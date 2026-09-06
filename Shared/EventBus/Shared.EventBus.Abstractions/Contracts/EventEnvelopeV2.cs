// ============================================================================
//  Project:        Shared.EventBus.Abstractions
//  File:           EventEnvelopeV2.cs
//  Created:        2026-08-08
//  Description:    Phase B.4 (Path B: Transformative Modernization). Carries
//                  every field EventEnvelope (V1) does, plus nullable
//                  facility/hazard enrichment fields. Additive-only:
//                  EventEnvelope is untouched and both types coexist --
//                  legacy subscribers keep consuming EventEnvelope, new
//                  subscribers consume this.
//
//                  FacilityId/IndustryName/Location/HazardType are typed as
//                  Guid?/string?/string?/string? (not a reference to
//                  Shared.FacilityDomain's FacilityId/HazardType types)
//                  deliberately: EventEnvelope's own header states this
//                  library is "transport-agnostic... lives in Shared so any
//                  module... can publish/consume without a [caller-specific]
//                  dependency," and every existing field is already a
//                  primitive for exactly that reason (Dapper/SQL-queue-
//                  column-friendly, no ORM). Taking a project reference on
//                  Shared.FacilityDomain here would break that invariant.
//                  All four are nullable because most events cannot resolve
//                  them yet -- see FacilityInferenceRules.cs/
//                  HazardInferenceRules.cs.
// ============================================================================
using System.Text.Json;

namespace Shared.EventBus.Abstractions.Contracts;

/// <summary>
/// Canonical, transport-agnostic V2 envelope: every EventEnvelope (V1) field, plus nullable
/// facility/hazard enrichment. Immutable once created; no post-publish mutation permitted.
/// </summary>
public sealed record EventEnvelopeV2
{
    /// <summary>UUIDv7-style unique identifier for this event instance.</summary>
    public required Guid EventId { get; init; }

    /// <summary>Canonical event type, format <c>&lt;Module&gt;.&lt;EventName&gt;</c> (e.g. "Accidents.AccidentCreatedV2").</summary>
    public required string EventType { get; init; }

    /// <summary>Schema version of the payload shape (1-based; increments on breaking change).</summary>
    public required int SchemaVersion { get; init; }

    /// <summary>CLR type name of the payload (simple name). Used by subscribers to select a deserializer/handler.</summary>
    public required string PayloadType { get; init; }

    /// <summary>Tenant identifier for multi-tenancy isolation. Required on every event.</summary>
    public required string TenantId { get; init; }

    /// <summary>Correlation identifier propagated from the triggering command/request.</summary>
    public required Guid CorrelationId { get; init; }

    /// <summary>Causation identifier: the specific event/command that caused this event.</summary>
    public required Guid CausationId { get; init; }

    /// <summary>UTC instant the underlying domain fact occurred (not when it was published).</summary>
    public required DateTime TimestampUtc { get; init; }

    /// <summary>Publishing/source module name (e.g. "PlantSafety").</summary>
    public required string SourceModule { get; init; }

    /// <summary>JSON-serialized payload. Schema is immutable once published for a given EventId.</summary>
    public required string Payload { get; init; }

    /// <summary>UTC instant this envelope was written to the queue. Set by the publisher/infrastructure.</summary>
    public DateTime EnqueuedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>The resolved facility for this event, if one could be inferred. Null for the large majority of events today -- see FacilityInferenceRules.cs.</summary>
    public Guid? FacilityId { get; init; }

    /// <summary>The resolved facility's industry name, if known.</summary>
    public string? IndustryName { get; init; }

    /// <summary>The resolved facility's location (free text), if known.</summary>
    public string? Location { get; init; }

    /// <summary>The inferred hazard type name (matches Shared.FacilityDomain.Abstractions.Hazards.HazardType's member names), if one could be classified -- see HazardInferenceRules.cs.</summary>
    public string? HazardType { get; init; }

    /// <summary>Builds an envelope from a strongly-typed integration event DTO, JSON-serializing the payload.</summary>
    public static EventEnvelopeV2 Create<TPayload>(
        TPayload payload,
        string tenantId,
        Guid correlationId,
        Guid causationId,
        DateTime timestampUtc,
        string sourceModule,
        string eventType,
        int schemaVersion,
        Guid? eventId = null,
        Guid? facilityId = null,
        string? industryName = null,
        string? location = null,
        string? hazardType = null)
        where TPayload : notnull
        => new()
        {
            EventId = eventId ?? Guid.NewGuid(),
            EventType = eventType,
            SchemaVersion = schemaVersion,
            PayloadType = typeof(TPayload).Name,
            TenantId = tenantId,
            CorrelationId = correlationId,
            CausationId = causationId,
            TimestampUtc = timestampUtc,
            SourceModule = sourceModule,
            Payload = JsonSerializer.Serialize(payload, typeof(TPayload)),
            FacilityId = facilityId,
            IndustryName = industryName,
            Location = location,
            HazardType = hazardType,
        };

    /// <summary>Deserializes <see cref="Payload"/> back into <typeparamref name="TPayload"/>.</summary>
    public TPayload DeserializePayload<TPayload>()
        => JsonSerializer.Deserialize<TPayload>(Payload)
           ?? throw new InvalidOperationException($"EventEnvelopeV2 {EventId} payload could not be deserialized as {typeof(TPayload).Name}.");
}

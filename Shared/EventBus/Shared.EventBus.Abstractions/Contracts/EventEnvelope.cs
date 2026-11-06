// ============================================================================
//  Project:        Shared.EventBus.Abstractions
//  File:           EventEnvelope.cs
//  Created:        2026-07-27
//  Description:    Canonical cross-module event envelope. Mirrors the field set of
//                  PlantSafety's intra-module canonical envelope (see
//                  project_plantsafety_patterns.md §4, "Canonical Event Envelope Fields")
//                  but is transport-agnostic and lives in Shared so any module (PlantSafety,
//                  Reporting, future modules) can publish/consume without a PlantSafety
//                  dependency. Dapper-friendly: every property is a primitive/string, so it
//                  maps 1:1 onto SQL queue table columns with no ORM involved.
// ============================================================================
using System.Text.Json;

namespace Shared.EventBus.Abstractions.Contracts;

/// <summary>
/// Canonical, transport-agnostic envelope wrapping a serialized cross-module integration
/// event payload. Immutable once created; no post-publish mutation permitted.
/// </summary>
public sealed record EventEnvelope
{
    /// <summary>UUIDv7-style unique identifier for this event instance.</summary>
    public required Guid EventId { get; init; }

    /// <summary>Canonical event type, format <c>&lt;Module&gt;.&lt;EventName&gt;</c> (e.g. "Incidents.IncidentManagementCreated").</summary>
    public required string EventType { get; init; }

    /// <summary>Schema version of the payload shape (1-based; increments on breaking change).</summary>
    public required int SchemaVersion { get; init; }

    /// <summary>CLR type name of the payload (simple name, e.g. "IncidentManagementCreatedIntegrationEvent"). Used by subscribers to select a deserializer/handler.</summary>
    public required string PayloadType { get; init; }

    /// <summary>Tenant identifier for multi-tenancy isolation. Required on every event (patterns.md §15).</summary>
    public required string TenantId { get; init; }

    /// <summary>Correlation identifier propagated from the triggering command/request.</summary>
    public required Guid CorrelationId { get; init; }

    /// <summary>Causation identifier: the specific event/command that caused this event.</summary>
    public required Guid CausationId { get; init; }

    /// <summary>UTC instant the underlying domain fact occurred (not when it was published).</summary>
    public required DateTime OccurredAtUtc { get; init; }

    /// <summary>Publishing/source module name (e.g. "PlantSafety").</summary>
    public required string SourceModule { get; init; }

    /// <summary>JSON-serialized payload. Schema is immutable once published for a given EventId.</summary>
    public required string Payload { get; init; }

    /// <summary>UTC instant this envelope was written to the queue. Set by the publisher/infrastructure.</summary>
    public DateTime EnqueuedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Builds an envelope from a strongly-typed integration event DTO, JSON-serializing the payload.
    /// </summary>
    public static EventEnvelope Create<TPayload>(
        TPayload payload,
        string tenantId,
        Guid correlationId,
        Guid causationId,
        DateTime occurredAtUtc,
        string sourceModule,
        string eventType,
        int schemaVersion,
        Guid? eventId = null)
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
            OccurredAtUtc = occurredAtUtc,
            SourceModule = sourceModule,
            Payload = JsonSerializer.Serialize(payload, typeof(TPayload)),
        };

    /// <summary>Deserializes <see cref="Payload"/> back into <typeparamref name="TPayload"/>.</summary>
    public TPayload DeserializePayload<TPayload>()
        => JsonSerializer.Deserialize<TPayload>(Payload)
           ?? throw new InvalidOperationException($"EventEnvelope {EventId} payload could not be deserialized as {typeof(TPayload).Name}.");
}

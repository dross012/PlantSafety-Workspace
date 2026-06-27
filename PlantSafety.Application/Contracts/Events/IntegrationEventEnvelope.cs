// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical integration event envelope written to the OutboxMessages table.
//   §4 (Canonical Event Architecture): domain events are wrapped in this envelope before commit.
//   §14.9 (Canonical Event Envelope Schema): all required fields present.
//   Immutable once created; no post-publish mutation permitted.
// SchemaVersion: 1
// Additive-only evolution: new optional fields may be added with defaults; no fields may be removed.

namespace PlantSafety.Application.Contracts.Events;

/// <summary>
/// Canonical envelope that wraps a domain event payload for outbox persistence and integration event publishing.
/// Written to the <c>OutboxMessages</c> table as part of the atomic Unit of Work commit.
/// The Outbox Processor reads these envelopes and publishes to Azure Service Bus.
/// </summary>
/// <remarks>
/// Initializes a new <see cref="IntegrationEventEnvelope"/> with all required fields.
/// </remarks>
/// <param name="eventId">The UUIDv7 event identifier.</param>
/// <param name="eventType">The canonical event type name.</param>
/// <param name="eventVersion">The schema version of the event.</param>
/// <param name="aggregateId">The identifier of the source aggregate.</param>
/// <param name="aggregateName">The canonical name of the source aggregate type.</param>
/// <param name="correlationId">The correlation identifier inherited from the triggering command.</param>
/// <param name="causationId">The causation identifier inherited from the triggering command.</param>
/// <param name="timestampUtc">The UTC timestamp when the domain event occurred.</param>
/// <param name="actorId">The actor who triggered the event.</param>
/// <param name="tenantId">The tenant identifier for multi-tenancy isolation.</param>
/// <param name="payload">The JSON-serialized event payload.</param>
public sealed class IntegrationEventEnvelope(
    Guid eventId,
    string eventType,
    int eventVersion,
    Guid aggregateId,
    string aggregateName,
    Guid correlationId,
    Guid causationId,
    DateTime timestampUtc,
    Guid actorId,
    Guid tenantId,
    string payload)
{

    /// <summary>Gets the unique identifier of this event (UUIDv7).</summary>
    public Guid EventId { get; } = eventId;

    /// <summary>Gets the canonical event type in format <c>&lt;Module&gt;.&lt;AggregateAction&gt;.v&lt;version&gt;</c>.</summary>
    public string EventType { get; } = eventType;

    /// <summary>Gets the schema version (starts at 1; increments on additive change).</summary>
    public int EventVersion { get; } = eventVersion;

    /// <summary>Gets the identifier of the aggregate that raised this event.</summary>
    public Guid AggregateId { get; } = aggregateId;

    /// <summary>Gets the canonical name of the source aggregate type.</summary>
    public string AggregateName { get; } = aggregateName;

    /// <summary>Gets the correlation identifier inherited from the triggering command.</summary>
    public Guid CorrelationId { get; } = correlationId;

    /// <summary>Gets the causation identifier inherited from the triggering command.</summary>
    public Guid CausationId { get; } = causationId;

    /// <summary>Gets the UTC timestamp when the domain event occurred.</summary>
    public DateTime TimestampUtc { get; } = timestampUtc;

    /// <summary>Gets the actor who triggered the event.</summary>
    public Guid ActorId { get; } = actorId;

    /// <summary>Gets the tenant identifier for multi-tenancy isolation.</summary>
    public Guid TenantId { get; } = tenantId;

    /// <summary>Gets the JSON-serialized event payload. Schema is immutable once published.</summary>
    public string Payload { get; } = payload;

    /// <summary>Gets or sets the UTC timestamp when this envelope was written to the outbox.</summary>
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC timestamp when the Outbox Processor published this envelope.</summary>
    public DateTime? PublishedAtUtc { get; set; }

    /// <summary>Gets or sets the number of publish attempts (bounded by retry policy).</summary>
    public int AttemptCount { get; set; }

    /// <summary>Gets or sets whether this envelope has been successfully published.</summary>
    public bool IsPublished { get; set; }
}

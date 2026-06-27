// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical marker interface for Integration Events published to Azure Service Bus.
//   Integration events cross module boundaries via the Outbox pattern.
//   Required envelope fields are mandated by §14.9 (Canonical Event Envelope Schema).
// SchemaVersion: 1
// Additive-only evolution: new fields may be added with defaults; no fields may be removed or renamed.

namespace PlantSafety.Application.Abstractions.Messaging;

/// <summary>
/// Canonical marker interface for integration events that cross module boundaries.
/// All integration events are published after successful commit (Publish-After-Commit Rule).
/// Events are immutable once created; no post-publish mutation is permitted.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>Gets the unique identifier of this event (UUIDv7 for deterministic time-ordering).</summary>
    Guid EventId { get; }

    /// <summary>Gets the canonical event type name in format <c>&lt;Module&gt;.&lt;AggregateAction&gt;.v&lt;version&gt;</c>.</summary>
    string EventType { get; }

    /// <summary>Gets the schema version of this event (starts at 1; increments on additive change only).</summary>
    int EventVersion { get; }

    /// <summary>Gets the identifier of the aggregate that raised this event.</summary>
    Guid AggregateId { get; }

    /// <summary>Gets the canonical name of the aggregate type.</summary>
    string AggregateName { get; }

    /// <summary>Gets the correlation identifier inherited from the triggering command.</summary>
    Guid CorrelationId { get; }

    /// <summary>Gets the causation identifier inherited from the triggering command (never regenerated).</summary>
    Guid CausationId { get; }

    /// <summary>Gets the UTC timestamp when the domain event occurred.</summary>
    DateTime TimestampUtc { get; }

    /// <summary>Gets the identifier of the actor who triggered the event.</summary>
    Guid ActorId { get; }

    /// <summary>Gets the tenant identifier for multi-tenancy isolation.</summary>
    Guid TenantId { get; }
}

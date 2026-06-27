/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               IDomainEvent.cs
    DESCRIPTION:        Marker interface for all domain events in the Intelligence domain layer.
                        Carries the full envelope contract so consumers never need to cast to the
                        concrete record type to read header fields. (Rule 2.13)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.Shared.Interfaces;

/// <summary>
/// Contract for all domain events raised by Intelligence aggregates.
/// Events are collected by the aggregate, written to the Outbox by the Unit of Work,
/// and never dispatched in-process.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique identifier of this event instance.</summary>
    Guid EventId { get; }

    /// <summary>The fully-qualified event type name (matches the concrete record class name).</summary>
    string EventType { get; }

    /// <summary>Schema version for this event type. Incremented on breaking payload changes.</summary>
    int EventVersion { get; }

    /// <summary>The identifier of the aggregate that raised this event.</summary>
    Guid AggregateId { get; }

    /// <summary>Propagated from the originating command for distributed tracing.</summary>
    Guid CorrelationId { get; }

    /// <summary>Identifier of the command or event that caused this event to be raised.</summary>
    Guid CausationId { get; }

    /// <summary>UTC timestamp when this event was raised.</summary>
    DateTime TimestampUtc { get; }
}

/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               DomainEvent.cs
    DESCRIPTION:        Abstract base record for all domain events in the Intelligence domain. Provides
                        default envelope fields. Concrete event records must be sealed, extend this base,
                        and override EventType with nameof(ConcreteEventClass). CausationId must be
                        inherited from the triggering command — never regenerated. (Rule 2.13)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. Envelope fields must never be removed.
==========================================================================================================================*/

using Intelligence.Domain.Shared.Interfaces;

namespace Intelligence.Domain.Shared.Abstractions;

/// <summary>
/// Abstract base for all Intelligence domain events. Concrete events must be sealed records
/// and must override <see cref="EventType"/> with <c>nameof(ConcreteEventClass)</c>.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public abstract string EventType { get; }

    /// <inheritdoc />
    public virtual int EventVersion => 1;

    /// <inheritdoc />
    public Guid AggregateId { get; init; }

    /// <inheritdoc />
    public Guid CorrelationId { get; init; }

    /// <summary>
    /// Identifier of the command or event that caused this domain event.
    /// Must be propagated from the triggering command — never regenerated with Guid.NewGuid().
    /// </summary>
    public Guid CausationId { get; init; }

    /// <inheritdoc />
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    /// <summary>The actor ID of the user or system principal that triggered this event. May be null for system events.</summary>
    public string? ActorId { get; init; }
}

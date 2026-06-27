/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IDomainEvent.cs
    DESCRIPTION:        Canonical domain event contract. All domain events MUST implement this interface. Enforces strong
                        typing, deterministic behavior, SIEM-safe metadata, and additive-only evolution.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. Existing members MUST NOT change semantics.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Events.Base;

/// <summary>
/// Canonical domain event contract.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Canonical event name (e.g., Routing.JobAssigned).
    /// MUST follow Canonical Event Naming Rules.
    /// </summary>
    EventName Name { get; }

    /// <summary>
    /// Canonical event version.
    /// MUST follow Canonical Event Versioning Rules.
    /// </summary>
    EventVersion Version { get; }

    /// <summary>
    /// Strong-typed metadata envelope.
    /// </summary>
    IDomainEventMetadata Metadata { get; }
}

/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               DomainEventBase.cs
    DESCRIPTION:        Canonical base class for all domain events. Provides strong-typed metadata, versioning, naming,
                        correlation, causation, and deterministic UTC timestamp behavior.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. New optional metadata MAY be added; existing fields MUST remain stable.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Events.Base;

/// <summary>
/// Canonical base class for all domain events.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public required EventName Name { get; init; }
    public required EventVersion Version { get; init; }
    public required IDomainEventMetadata Metadata { get; init; }
}

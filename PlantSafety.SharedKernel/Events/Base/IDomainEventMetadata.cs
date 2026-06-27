/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IDomainEventMetadata.cs
    DESCRIPTION:        Strong-typed metadata envelope for domain events. Includes correlation, causation, timestamps,
                        actor identity, module identity, and deterministic lineage fields.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Events.Base;

/// <summary>
/// Strong-typed metadata envelope for domain events.
/// </summary>
public interface IDomainEventMetadata
{
    string EventId { get; }
    EventCorrelation Correlation { get; }
    EventCausation Causation { get; }
    string Module { get; }
    string Actor { get; }
    DateTime OccurredUtc { get; }
}

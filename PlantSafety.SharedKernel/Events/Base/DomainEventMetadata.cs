/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               DomainEventMetadata.cs
    DESCRIPTION:        Canonical metadata implementation for domain events. Enforces deterministic UTC timestamps,
                        correlation, causation, and SIEM-safe identity semantics.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Events.Base;

/// <summary>
/// Canonical metadata implementation for domain events.
/// </summary>
public sealed record DomainEventMetadata(
    string EventId,
    EventCorrelation Correlation,
    EventCausation Causation,
    string Module,
    string Actor,
    DateTime OccurredUtc
) : IDomainEventMetadata;

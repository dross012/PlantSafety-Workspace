// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainerPlacardedEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when placards are computed and applied to a HazmatContainer.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Events;

/// <summary>Raised when placards are applied (MarkedAndLabeled -&gt; PlacardingCalculated).</summary>
public sealed record HazmatContainerPlacardedEvent(
    HazmatContainerEventId EventId,
    HazmatContainerId HazmatContainerId,
    IReadOnlyList<string> Placards,
    UtcTimestamp PlacardedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazmatContainerDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazmatContainerPlacarded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PlacardedAtUtc;
}

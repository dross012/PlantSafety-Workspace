// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainerRetiredEvent.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Raised when a HazmatContainer is retired from active use.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Events;

/// <summary>Raised when a container is retired (any non-terminal status -&gt; Retired).</summary>
public sealed record HazmatContainerRetiredEvent(
    HazmatContainerEventId EventId,
    HazmatContainerId HazmatContainerId,
    UtcTimestamp RetiredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IHazmatContainerDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "HazmatContainerRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RetiredAtUtc;
}
